using System;
using System.Collections.Generic;
using Slotwise.Domain.Base;

namespace Slotwise.Domain.Sessions.ValueObjects
{
    /// <summary>
    /// The total capacity of a session — how many bookings can be confirmed at once.
    /// Needed as its own type (instead of a raw int) so "zero or negative capacity" can never
    /// exist as a valid value anywhere in the domain, enforced once here rather than at every call site.
    /// </summary>
    public sealed class SeatCount : ValueObject<SeatCount>
    {
        /// <summary>The number of seats available. Private setter keeps the value object immutable after construction.</summary>
        public int Value { get; private set; }

        /// <summary>Parameterless constructor reserved for ORMs (e.g. EF Core), which materialize value objects via reflection and never call the public constructor.</summary>
        private SeatCount() { }

        /// <summary>Creates a seat count, throwing if the value is not a positive number — this is the one place that guards against an invalid capacity being created.</summary>
        public SeatCount(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Seat count must be greater than zero.");

            Value = value;
        }

        /// <summary>Two seat counts are equal when their values match. Required by <see cref="ValueObject"/> to define what "equal" means for this type.</summary>
        /// Yield here is needed because multiple types of valueobject use this method to get components and is returned as a list  of objects. 
        /// Objects can be integers, strings too. Using yield you can skip heap allocation and return directly multiple items it goes through list index for each yield call.
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        /// <summary>Returns the seat count as a string. Overridden so logging/string interpolation shows the number directly instead of the type name.</summary>
        public override string ToString() => Value.ToString();
    }
}
