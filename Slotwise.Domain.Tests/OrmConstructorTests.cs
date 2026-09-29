using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Tests;

/// <summary>
/// EF Core materializes these types through a non-public parameterless constructor.
/// Removing one compiles fine but breaks persistence at runtime, so these tests guard the contract.
/// </summary>
public class OrmConstructorTests
{
    [Theory]
    [InlineData(typeof(Session))]
    [InlineData(typeof(Booking))]
    [InlineData(typeof(EmailAddress))]
    [InlineData(typeof(SeatCount))]
    [InlineData(typeof(TimeSlot))]
    public void Type_CanBeCreatedThroughItsPrivateConstructor(Type type)
    {
        var instance = Activator.CreateInstance(type, nonPublic: true);

        Assert.NotNull(instance);
    }
}
