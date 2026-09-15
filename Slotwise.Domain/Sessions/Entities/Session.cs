using System;
using System.Collections.Generic;
using System.Linq;
using Slotwise.Domain.Base;
using Slotwise.Domain.Sessions.Events;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Sessions.Entities
{
    /// <summary>
    /// The aggregate root for the booking domain. Owns the invariant that confirmed
    /// bookings can never exceed <see cref="SeatCount"/>, and is the only entry point
    /// for creating, cancelling, or promoting bookings underneath it. Needed as a single
    /// choke point so the capacity rule can be verified/tested in one place instead of
    /// trusting every caller to check it correctly themselves.
    /// </summary>
    public sealed class Session : AggregateRoot
    {
        /// <summary>The name of the session (e.g. a class or workshop title). Private setter forces changes through explicit, validated methods rather than direct field mutation.</summary>
        public string Title { get; private set; } = string.Empty;

        /// <summary>When the session takes place.</summary>
        public TimeSlot TimeSlot { get; private set; } = null!;

        /// <summary>The session's total capacity. Read by <see cref="HasAvailableCapacity"/> to enforce the invariant.</summary>
        public SeatCount SeatCount { get; private set; } = null!;

        private readonly List<Booking> bookings = new List<Booking>();

        /// <summary>
        /// All bookings made against this session, confirmed, waitlisted, or cancelled.
        /// Exposed as read-only so outside code can inspect bookings but can't add or remove
        /// them directly, which would bypass the capacity check and event raising in <see cref="Book"/>.
        /// </summary>
        public IReadOnlyList<Booking> Bookings => bookings.AsReadOnly();

        /// <summary>Parameterless constructor reserved for ORMs (e.g. EF Core), which materialize aggregates via reflection and never call the public constructor.</summary>
        private Session() { }

        /// <summary>Creates a new session, throwing if the title is missing or the value objects are null — guarantees a Session can never exist in an invalid state.</summary>
        public Session(string title, TimeSlot timeSlot, SeatCount seatCount) : base(Guid.NewGuid())
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            Title = title;
            TimeSlot = timeSlot ?? throw new ArgumentNullException(nameof(timeSlot));
            SeatCount = seatCount ?? throw new ArgumentNullException(nameof(seatCount));
        }

        /// <summary>Counts how many bookings currently hold a confirmed seat. Kept private since it's just an internal building block for the capacity check, not something callers need directly.</summary>
        private int ConfirmedCount() => bookings.Count(b => b.Status == BookingStatus.Confirmed);

        /// <summary>Whether there is room to confirm another booking right now. This is the actual invariant check, reused by both <see cref="Book"/> and <see cref="PromoteFromWaitlist"/>.</summary>
        private bool HasAvailableCapacity() => ConfirmedCount() < SeatCount.Value;

        /// <summary>
        /// Books a seat for the given attendee. Confirms the booking if capacity allows,
        /// otherwise waitlists it. This is the single place the capacity invariant is enforced —
        /// no booking can become Confirmed anywhere else in the codebase.
        /// </summary>
        public Booking Book(EmailAddress email)
        {
            var status = HasAvailableCapacity() ? BookingStatus.Confirmed : BookingStatus.Waitlisted;
            var booking = new Booking(Id, email, status);
            bookings.Add(booking);

            if (status == BookingStatus.Confirmed)
                RaiseEvent(new BookingConfirmed(Id, booking.Id, email.Value));

            return booking;
        }

        /// <summary>
        /// Cancels a booking on this session. Always raises <see cref="BookingCancelled"/>;
        /// additionally raises <see cref="SpotOpened"/> only if the cancelled booking was
        /// occupying a confirmed seat — needed because only that case actually frees capacity
        /// for someone on the waitlist, and that's the signal the messaging/notification flow cares about.
        /// </summary>
        public void CancelBooking(Guid bookingId)
        {
            var booking = bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking is null)
                throw new InvalidOperationException("Booking not found on this session.");

            var wasConfirmed = booking.Status == BookingStatus.Confirmed;
            booking.Cancel();
            RaiseEvent(new BookingCancelled(Id, booking.Id, booking.EmailAddress.Value));

            if (wasConfirmed)
                RaiseEvent(new SpotOpened(Id));
        }

        /// <summary>
        /// Promotes a specific waitlisted booking into a confirmed seat. Re-checks capacity
        /// before promoting — needed so the invariant holds even if this is ever called out of
        /// the expected cancel-then-promote order, instead of just trusting the caller.
        /// </summary>
        public void PromoteFromWaitlist(Guid bookingId)
        {
            var booking = bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking is null)
                throw new InvalidOperationException("Booking not found on this session.");

            if (!HasAvailableCapacity())
                throw new InvalidOperationException("No available capacity to promote a booking.");

            booking.Promote();
            RaiseEvent(new WaitlistPromoted(Id, booking.Id, booking.EmailAddress.Value));
        }
    }
}
