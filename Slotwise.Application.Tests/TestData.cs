using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Application.Tests;

internal static class TestData
{
    public static Session NewSession(int capacity = 2)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new Session("Yoga", new TimeSlot(start, start.AddHours(1)), new SeatCount(capacity));
    }
}
