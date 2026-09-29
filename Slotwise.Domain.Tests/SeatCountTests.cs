using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Tests;

public class SeatCountTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    public void Constructor_PositiveValue_StoresValue(int value)
    {
        Assert.Equal(value, new SeatCount(value).Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_ZeroOrNegative_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SeatCount(value));
    }

    [Fact]
    public void ToString_ReturnsTheNumber()
    {
        Assert.Equal("12", new SeatCount(12).ToString());
    }

    [Fact]
    public void Equality_SameValue_IsEqual()
    {
        Assert.Equal(new SeatCount(5), new SeatCount(5));
    }

    [Fact]
    public void Equality_DifferentValue_IsNotEqual()
    {
        Assert.NotEqual(new SeatCount(5), new SeatCount(6));
    }
}
