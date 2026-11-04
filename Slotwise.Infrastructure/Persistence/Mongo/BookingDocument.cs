using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Slotwise.Application.Sessions.DTOs;

namespace Slotwise.Infrastructure.Persistence.Mongo
{
    /// <summary>
    /// A booking as it is stored inside a <see cref="SessionDocument"/>. Embedded rather than kept in its own
    /// collection, because the read model exists to return a whole session in one read with no joins.
    /// A plain class with setters (not a record) because the Mongo driver needs to populate it when reading.
    /// </summary>
    public sealed class BookingDocument
    {
        /// <summary>Stored as a standard UUID so every driver and tool reads the same value (see <see cref="SessionDocument.Id"/>).</summary>
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        /// <summary>Kept as the same string the DTO uses ("Confirmed", "Waitlisted", "Cancelled"), so no conversion is needed on the way out.</summary>
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public static BookingDocument From(BookingDTO dto)
        {
            return new BookingDocument
            {
                Id = dto.Id,
                Email = dto.Email,
                Status = dto.Status,
                CreatedAt = SessionDocument.AsUtc(dto.CreatedAt)
            };
        }

        public BookingDTO ToDto()
        {
            return new BookingDTO(Id, Email, Status, CreatedAt);
        }
    }
}
