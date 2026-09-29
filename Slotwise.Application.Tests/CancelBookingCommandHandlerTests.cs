using Moq;
using Slotwise.Application.Sessions.Commands.CancelBooking;
using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.Services;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Application.Tests;

public class CancelBookingCommandHandlerTests
{
    private readonly Mock<ISessionRepository> _repository = new();
    private readonly CancelBookingCommandHandler _handler;

    public CancelBookingCommandHandlerTests()
    {
        _handler = new CancelBookingCommandHandler(_repository.Object, new WaitlistPromotionPolicy());
    }

    private Session GivenSession(int capacity)
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

        await Assert.ThrowsAsync<SessionNotFoundException>(
            () => _handler.Handle(new CancelBookingCommand(id, Guid.NewGuid())));

        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_UnknownBooking_ThrowsAndSavesNothing()
    {
        var session = GivenSession(capacity: 1);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new CancelBookingCommand(session.Id, Guid.NewGuid())));

        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_CancellingConfirmedBookingWithWaitlist_PromotesLongestWaiting()
    {
        var session = GivenSession(capacity: 1);
        var confirmed = session.Book(new EmailAddress("a@example.com"));
        var firstInLine = session.Book(new EmailAddress("b@example.com"));
        var secondInLine = session.Book(new EmailAddress("c@example.com"));

        await _handler.Handle(new CancelBookingCommand(session.Id, confirmed.Id));

        Assert.Equal(BookingStatus.Cancelled, confirmed.Status);
        Assert.Equal(BookingStatus.Confirmed, firstInLine.Status);
        Assert.Equal(BookingStatus.Waitlisted, secondInLine.Status);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CancellingConfirmedBookingWithEmptyWaitlist_JustCancelsAndSaves()
    {
        var session = GivenSession(capacity: 2);
        var confirmed = session.Book(new EmailAddress("a@example.com"));

        await _handler.Handle(new CancelBookingCommand(session.Id, confirmed.Id));

        Assert.Equal(BookingStatus.Cancelled, confirmed.Status);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CancellingWaitlistedBooking_DoesNotPromoteAnyone()
    {
        var session = GivenSession(capacity: 1);
        var confirmed = session.Book(new EmailAddress("a@example.com"));
        var waitlistedFirst = session.Book(new EmailAddress("b@example.com"));
        var waitlistedSecond = session.Book(new EmailAddress("c@example.com"));

        await _handler.Handle(new CancelBookingCommand(session.Id, waitlistedFirst.Id));

        Assert.Equal(BookingStatus.Cancelled, waitlistedFirst.Status);
        Assert.Equal(BookingStatus.Confirmed, confirmed.Status);
        Assert.Equal(BookingStatus.Waitlisted, waitlistedSecond.Status);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
