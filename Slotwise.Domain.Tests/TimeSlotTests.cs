using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Tests;

public class TimeSlotTests
{
    private static readonly DateTime Start = new(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_EndAfterStart_StoresValuesAndComputesDuration()
    {
        var slot = new TimeSlot(Start, Start.AddMinutes(90));

        Assert.Equal(Start, slot.Start);
        Assert.Equal(Start.AddMinutes(90), slot.End);
        Assert.Equal(TimeSpan.FromMinutes(90), slot.Duration);
    }

    [Fact]
    public void Constructor_EndEqualsStart_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TimeSlot(Start, Start));
    }

    [Fact]
    public void Constructor_EndBeforeStart_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TimeSlot(Start, Start.AddMinutes(-1)));
    }

    [Fact]
    public void Equality_SameStartAndEnd_IsEqual()
    {
        Assert.Equal(new TimeSlot(Start, Start.AddHours(1)), new TimeSlot(Start, Start.AddHours(1)));
    }

    [Fact]
    public void Equality_DifferentEnd_IsNotEqual()
    {
        Assert.NotEqual(new TimeSlot(Start, Start.AddHours(1)), new TimeSlot(Start, Start.AddHours(2)));
    }
}
