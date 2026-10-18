using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Slotwise.Application.Sessions.DTOs;

namespace Slotwise.Infrastructure.Persistence.Mongo
{
    /// <summary>
    /// The read model's copy of a session: one document holding everything the API returns for it,
    /// bookings embedded and the counts precomputed, so a query is a single lookup with no joins or counting.
    /// It deliberately mirrors <see cref="SessionDTO"/>; it is a stored shape, not a domain object, and lives in
    /// Infrastructure so the rest of the system never knows Mongo is involved.
    /// A plain class with setters (not a record) because the Mongo driver needs to populate it when reading.
    /// </summary>
    public sealed class SessionDocument
    {
        /// <summary>
        /// The session's id, stored as a standard UUID (BSON binary subtype 4). The representation is pinned explicitly
        /// because the legacy ones are not portable between drivers, and changing it later would orphan stored data.
        /// </summary>
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int Capacity { get; set; }

        public int ConfirmedCount { get; set; }

        public int WaitlistCount { get; set; }

        public List<BookingDocument> Bookings { get; set; } = new();

        /// <summary>Builds the document that represents the given session state.</summary>
        public static SessionDocument From(SessionDTO dto)
        {
            return new SessionDocument
            {
                Id = dto.Id,
                Title = dto.Title,
                Start = AsUtc(dto.Start),
                End = AsUtc(dto.End),
                Capacity = dto.Capacity,
                ConfirmedCount = dto.ConfirmedCount,
                WaitlistCount = dto.WaitlistCount,
                Bookings = dto.Bookings.Select(BookingDocument.From).ToList()
            };
        }

        /// <summary>Turns the stored document back into the DTO the API returns.</summary>
        public SessionDTO ToDto()
        {
            return new SessionDTO(
                Id,
                Title,
                Start,
                End,
                Capacity,
                ConfirmedCount,
                WaitlistCount,
                Bookings.Select(b => b.ToDto()).ToList());
        }

        /// <summary>
        /// Marks a date as UTC without moving it. Every date in this system is UTC, but SQL Server hands them back with
        /// no kind at all, and the Mongo driver treats a kind-less date as local time and shifts it by the machine's offset.
        /// </summary>
        internal static DateTime AsUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Local
                ? value.ToUniversalTime()
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}
