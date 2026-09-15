using System.Collections.Generic;
using System.Linq;

namespace Slotwise.Domain.Base
{
    /// <summary>
    /// Base class for domain objects with no identity of their own — they are defined
    /// entirely by the combination of their field values (e.g. an email address or a time slot).
    /// Needed so <see cref="SeatCount"/>, <see cref="TimeSlot"/>, <see cref="EmailAddress"/>, etc.
    /// don't each have to reimplement equality — they just declare which fields matter.
    /// </summary>
    public abstract class ValueObject<T> : IEquatable<ValueObject<T>> where T : ValueObject<T>
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public bool Equals(ValueObject<T>? other)
        {
            if (other is null || other.GetType() != this.GetType())
                return false;

            return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != this.GetType())
                return false;

            return this.Equals(obj as ValueObject<T>);
        }

        public override int GetHashCode()
        {
            return this.GetEqualityComponents()
                .Select(component => component?.GetHashCode() ?? 0)
                .Aggregate((a, b) => a ^ b);
        }

        public static bool operator ==(ValueObject<T>? left, ValueObject<T>? right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(ValueObject<T>? left, ValueObject<T>? right) => !(left == right);
    }
}
