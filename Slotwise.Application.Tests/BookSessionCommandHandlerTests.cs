using Moq;
using Slotwise.Application.Sessions.Commands.BookSession;
using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Application.Tests;

public class BookSessionCommandHandlerTests
{
    private readonly Mock<ISessionRepository> _repository = new();
    private readonly BookSessionCommandHandler _handler;

    public BookSessionCommandHandlerTests()
    {
        _handler = new BookSessionCommandHandler(_repository.Object);
    }

    private Session GivenSession(int capacity = 2)
    {
        var session = TestData.NewSession(capacity);
        _repository.Setup(r => r.GetByIdAsync(session.Id)).ReturnsAsync(session);
        return session;
    }

    [Fact]
    public async Task Handle_UnknownSession_ThrowsNotFoundAndSavesNothing()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Session?)null);

        var ex = await Assert.ThrowsAsync<SessionNotFoundException>(
            () => _handler.Handle(new BookSessionCommand(id, "a@example.com")));

        Assert.Contains(id.ToString(), ex.Message);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_SeatAvailable_ReturnsConfirmedBookingAndSaves()
    {
        var session = GivenSession(capacity: 2);

        var dto = await _handler.Handle(new BookSessionCommand(session.Id, "Sam@Example.com"));

        var booking = Assert.Single(session.Bookings);
        Assert.Equal(booking.Id, dto.Id);
        Assert.Equal("sam@example.com", dto.Email);
        Assert.Equal("Confirmed", dto.Status);
        Assert.Equal(booking.CreatedAt, dto.CreatedAt);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SessionFull_ReturnsWaitlistedBooking()
    {
        var session = GivenSession(capacity: 1);
        session.Book(new EmailAddress("first@example.com"));

        var dto = await _handler.Handle(new BookSessionCommand(session.Id, "second@example.com"));

        Assert.Equal("Waitlisted", dto.Status);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailAlreadyBooked_ThrowsAndSavesNothing()
    {
        var session = GivenSession();
        session.Book(new EmailAddress("a@example.com"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new BookSessionCommand(session.Id, "a@example.com")));

        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ThrowsAndSavesNothing()
    {
        var session = GivenSession();

        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(new BookSessionCommand(session.Id, "not-an-email")));

        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
