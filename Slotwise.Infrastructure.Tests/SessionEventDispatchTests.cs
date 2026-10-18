using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.Events;
using Slotwise.Domain.Sessions.ValueObjects;
using Slotwise.Infrastructure.Persistence.Repositories;

namespace Slotwise.Infrastructure.Tests;

[Collection(SqlServerCollection.Name)]
public class SessionEventDispatchTests(SqlServerFixture db)
{
    private static Session NewSession(int capacity)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new Session("Yoga", new TimeSlot(start, start.AddHours(1)), new SeatCount(capacity));
    }

    [Fact]
    public async Task SaveChanges_PublishesTheRaisedEvents_AndClearsThemFromTheSession()
    {
        var session = NewSession(capacity: 1);
        session.Book(new EmailAddress("a@example.com"));
        var dispatcher = new RecordingDomainEventDispatcher();
        var context = db.CreateContext();

        try
        {
            var repository = new SessionRepository(context, dispatcher);
            repository.Add(session);
            await repository.SaveChangesAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }

        Assert.Single(dispatcher.Dispatched.OfType<SessionCreated>());
        Assert.Single(dispatcher.Dispatched.OfType<BookingConfirmed>());
        Assert.Empty(session.DomainEvents);
    }

    [Fact]
    public async Task WhenTheSaveFails_NothingIsPublished_AndTheEventsStayOnTheSession()
    {
        var session = NewSession(capacity: 1);
        var setup = db.CreateContext();

        try
        {
            var repository = new SessionRepository(setup, new RecordingDomainEventDispatcher());
            repository.Add(session);
            await repository.SaveChangesAsync();
        }
        finally
        {
            if (setup is not null)
            {
                await setup.DisposeAsync();
            }
        }

        var contextA = db.CreateContext();
        var contextB = db.CreateContext();
        var dispatcherB = new RecordingDomainEventDispatcher();
        Session? sessionB;

        try
        {
            var repositoryA = new SessionRepository(contextA, new RecordingDomainEventDispatcher());
            var repositoryB = new SessionRepository(contextB, dispatcherB);

            var sessionA = await repositoryA.GetByIdAsync(session.Id);
            sessionB = await repositoryB.GetByIdAsync(session.Id);

            sessionA!.Book(new EmailAddress("a@example.com"));
            sessionB!.Book(new EmailAddress("b@example.com"));

            await repositoryA.SaveChangesAsync();
            await Assert.ThrowsAsync<SessionConcurrencyException>(() => repositoryB.SaveChangesAsync());
        }
        finally
        {
            if (contextA is not null)
            {
                await contextA.DisposeAsync();
            }

            if (contextB is not null)
            {
                await contextB.DisposeAsync();
            }
        }

        Assert.Empty(dispatcherB.Dispatched);
        Assert.NotEmpty(sessionB!.DomainEvents);
    }
}
