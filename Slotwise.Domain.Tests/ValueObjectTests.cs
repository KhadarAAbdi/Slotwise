using Slotwise.Domain.Base;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Tests;

public class ValueObjectTests
{
    private sealed class Pair : ValueObject<Pair>
    {
        private readonly string? _first;
        private readonly string _second;

        public Pair(string? first, string second)
        {
            _first = first;
            _second = second;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _first!;
            yield return _second;
        }
    }

    private sealed class Other : ValueObject<Other>
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return 5;
        }
    }

    [Fact]
    public void Equals_SameComponents_ReturnsTrue()
    {
        Assert.True(new SeatCount(5).Equals(new SeatCount(5)));
    }

    [Fact]
    public void Equals_DifferentComponents_ReturnsFalse()
    {
        Assert.False(new SeatCount(5).Equals(new SeatCount(6)));
    }

    [Fact]
    public void EqualsObject_Null_ReturnsFalse()
    {
        Assert.False(new SeatCount(5).Equals((object?)null));
    }

    [Fact]
    public void EqualsTyped_Null_ReturnsFalse()
    {
        Assert.False(new SeatCount(5).Equals((ValueObject<SeatCount>?)null));
    }

    [Fact]
    public void EqualsObject_DifferentType_ReturnsFalse()
    {
        Assert.False(new SeatCount(5).Equals("5"));
    }

    [Fact]
    public void EqualsObject_SameTypeSameComponents_ReturnsTrue()
    {
        Assert.True(new SeatCount(5).Equals((object)new SeatCount(5)));
    }

    [Fact]
    public void EqualsTyped_SameComponentsButDifferentRuntimeType_ReturnsFalse()
    {
        var pair = new Pair("a", "b");
        var other = new Other();

        Assert.False(other.Equals((object)pair));
    }

    [Fact]
    public void EqualityOperator_BothNull_IsTrue()
    {
        SeatCount? left = null;
        SeatCount? right = null;

        Assert.True(left == right);
    }

    [Fact]
    public void EqualityOperator_OneNull_IsFalse()
    {
        SeatCount? value = new SeatCount(5);

        Assert.False(value == null);
        Assert.False(null == value);
    }

    [Fact]
    public void EqualityOperator_EqualValues_IsTrue()
    {
        Assert.True(new SeatCount(5) == new SeatCount(5));
    }

    [Fact]
    public void InequalityOperator_DifferentValues_IsTrue()
    {
        Assert.True(new SeatCount(5) != new SeatCount(6));
        Assert.False(new SeatCount(5) != new SeatCount(5));
    }

    [Fact]
    public void GetHashCode_EqualValueObjects_HaveSameHash()
    {
        Assert.Equal(new TimeSlot(new DateTime(2030, 1, 1), new DateTime(2030, 1, 2)).GetHashCode(),
                     new TimeSlot(new DateTime(2030, 1, 1), new DateTime(2030, 1, 2)).GetHashCode());
    }

    [Fact]
    public void GetHashCode_NullComponent_TreatedAsZero()
    {
        var withNull = new Pair(null, "b");

        var ex = Record.Exception(() => withNull.GetHashCode());

        Assert.Null(ex);
    }
}
