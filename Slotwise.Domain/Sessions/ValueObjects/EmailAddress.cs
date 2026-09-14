using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Slotwise.Domain.Base;

namespace Slotwise.Domain.Sessions.ValueObjects
{
    /// <summary>
    /// A validated, normalized email address used to identify a booking's attendee.
    /// Needed so an invalid email can never exist as a value anywhere in the domain — validation
    /// happens once here instead of being repeated (or forgotten) at every place an email is used.
    /// </summary>
    public sealed class EmailAddress : ValueObject<EmailAddress>
    {
        private static readonly Regex Pattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        /// <summary>The normalized (trimmed, lowercased) email address value. Private setter keeps the value object immutable after construction.</summary>
        public string Value { get; private set; } = string.Empty;

        /// <summary>Parameterless constructor reserved for ORMs (e.g. EF Core), which materialize value objects via reflection and never call the public constructor.</summary>
        private EmailAddress() { }

        /// <summary>Creates an email address, throwing if the value is missing or not a valid email shape — this is the one place that guards against a bad email ever being created.</summary>
        public EmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Pattern.IsMatch(value))
                throw new ArgumentException("A valid email address is required.", nameof(value));

            Value = value.Trim().ToLowerInvariant();
        }

        /// <summary>Two email addresses are equal when their normalized values match. Required by <see cref="ValueObject"/> to define what "equal" means for this type.</summary>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        /// <summary>Returns the normalized email address value. Overridden so logging/string interpolation shows the email directly instead of the type name.</summary>
        public override string ToString() => Value;
    }
}
