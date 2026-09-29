using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Tests;

public class EntityTests
{
    private static Session CreateSession()
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new Session("Yoga", new TimeSlot(start, start.AddHours(1)), new SeatCount(2));
    }

    [Fact]
    public void Equals_SameReference_IsTrue()
    {
        var session = CreateSession();

        Assert.True(session.Equals(session));
    }

    [Fact]
    public void Equals_DifferentInstancesOfSameType_IsFalse()
    {
        Assert.False(CreateSession().Equals(CreateSession()));
    }

    [Fact]
    public void Equals_Null_IsFalse()
    {
        Assert.False(CreateSession().Equals(null));
    }

    [Fact]
    public void Equals_NonEntity_IsFalse()
    {
        Assert.False(CreateSession().Equals("not an entity"));
    }

    [Fact]
    public void Equals_DifferentEntityTypes_IsFalse()
    {
        var session = CreateSession();
        var booking = session.Book(new EmailAddress("a@example.com"));

        Assert.False(session.Equals(booking));
    }

    [Fact]
    public void GetHashCode_SameEntity_IsStable()
    {
        var session = CreateSession();

        Assert.Equal(session.GetHashCode(), session.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentEntities_Differ()
    {
        Assert.NotEqual(CreateSession().GetHashCode(), CreateSession().GetHashCode());
    }
}
