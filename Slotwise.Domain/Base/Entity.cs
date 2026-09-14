using System;

namespace Slotwise.Domain.Base
{
    /// <summary>
    /// Base class for domain objects that have a stable identity (a <see cref="Guid"/>)
    /// rather than being defined by their attribute values. Two entities are equal
    /// when they represent the same conceptual thing, even if their other fields differ.
    /// Needed because the default <see cref="object.Equals(object?)"/> compares by reference,
    /// which would treat two loads of "the same" booking from the database as unequal.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>The identity of this entity. Immutable once assigned, since an entity's identity should never change.</summary>
        public Guid Id { get; private set; }

        /// <summary>Parameterless constructor reserved for ORMs (e.g. EF Core), which materialize entities via reflection and never call the public constructor.</summary>
        protected Entity() { }

        /// <summary>Creates an entity with the given identity. Needed so every entity has an id the moment it exists, before it's ever persisted.</summary>
        protected Entity(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Two entities are equal when they are the same runtime type and share the same <see cref="Id"/>.
        /// Overridden because entity equality in DDD is about identity, not matching field values.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not Entity other) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            return Id == other.Id;
        }

        /// <summary>
        /// Hash code derived from the runtime type and <see cref="Id"/>, consistent with <see cref="Equals(object?)"/>.
        /// Needed because collections like <see cref="Dictionary{TKey,TValue}"/> and <see cref="HashSet{T}"/>
        /// use this to find the right bucket before calling Equals — if it disagreed with Equals,
        /// lookups could silently fail to find entities that are actually equal.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    }
}
