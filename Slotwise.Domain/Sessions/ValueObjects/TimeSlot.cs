using System;
using System.Collections.Generic;
using Slotwise.Domain.Base;

namespace Slotwise.Domain.Sessions.ValueObjects
{
    /// <summary>
    /// The start and end time during which a session takes place.
    /// Needed as one cohesive type instead of two loose DateTime fields, so "end before start"
    /// can never exist as a valid value and Start/End can never drift out of sync with each other.
    /// </summary>
    public sealed class TimeSlot : ValueObject<TimeSlot>
    {
        /// <summary>When the session starts. Private setter keeps the value object immutable after construction.</summary>
        public DateTime Start { get; private set; }

        /// <summary>When the session ends. Private setter keeps the value object immutable after construction.</summary>
        public DateTime End { get; private set; }

        /// <summary>How long the session lasts. Computed rather than stored, so it can never disagree with Start/End.</summary>
        public TimeSpan Duration => End - Start;

        /// <summary>Parameterless constructor reserved for ORMs (e.g. EF Core), which materialize value objects via reflection and never call the public constructor.</summary>
        private TimeSlot() { }

        /// <summary>Creates a time slot, throwing if the end time is not after the start time — this is the one place that guards against an invalid range being created.</summary>
        public TimeSlot(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ArgumentException("End time must be after the start time.", nameof(end));

            Start = start;
            End = end;
        }

        /// <summary>Two time slots are equal when their start and end times match. Required by <see cref="ValueObject"/> to define what "equal" means for this type.</summary>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Start;
            yield return End;
        }
    }
}
