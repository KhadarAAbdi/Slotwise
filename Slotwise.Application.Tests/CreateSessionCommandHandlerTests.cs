using Moq;
using Slotwise.Application.Sessions.Commands.CreateSession;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Sessions.Entities;

namespace Slotwise.Application.Tests;

public class CreateSessionCommandHandlerTests
{
    private static readonly DateTime Start = DateTime.UtcNow.AddDays(1);

    private readonly Mock<ISessionRepository> _repository = new();
    private readonly CreateSessionCommandHandler _handler;

    public CreateSessionCommandHandlerTests()
    {
        _handler = new CreateSessionCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsSessionAndReturnsItsId()
    {
        Session? added = null;
        _repository.Setup(r => r.Add(It.IsAny<Session>()))
            .Callback<Session>(s => added = s);

        var id = await _handler.Handle(new CreateSessionCommand("Yoga", Start, Start.AddHours(1), 10));

        Assert.NotNull(added);
        Assert.Equal(added.Id, id);
        Assert.Equal("Yoga", added.Title);
        Assert.Equal(10, added.SeatCount.Value);
        Assert.Equal(Start, added.TimeSlot.Start);
        Assert.Equal(Start.AddHours(1), added.TimeSlot.End);
        _repository.Verify(r => r.Add(added), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCapacity_ThrowsAndPersistsNothing()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _handler.Handle(new CreateSessionCommand("Yoga", Start, Start.AddHours(1), 0)));

        _repository.Verify(r => r.Add(It.IsAny<Session>()), Times.Never);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_EndBeforeStart_ThrowsAndPersistsNothing()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(new CreateSessionCommand("Yoga", Start, Start.AddHours(-1), 10)));

        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_BlankTitle_ThrowsAndPersistsNothing()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(new CreateSessionCommand("  ", Start, Start.AddHours(1), 10)));

        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
