using System.Linq;
using Slotwise.Domain.Sessions.Entities;

namespace Slotwise.Domain.Sessions.Services
{
    /// <summary>
    /// Domain service that decides who gets the next open seat. Exists as a standalone service
    /// rather than a method on <see cref="Session"/> or <see cref="Booking"/> because "who's next"
    /// is a policy that reasons across the whole waitlist — it doesn't naturally belong to a single
    /// booking, and keeping it separate means the ordering rule (FIFO today) can change independently of <see cref="Session"/>.
    /// </summary>
    public class WaitlistPromotionPolicy
    {
        /// <summary>
        /// Promotes the longest-waiting waitlisted booking (FIFO by creation time) on the
        /// given session, if one exists and capacity allows. Returns whether a promotion happened,
        /// so a caller (eventually a notification worker reacting to <see cref="Events.SpotOpened"/>)
        /// knows whether there's actually anyone to notify.
        /// </summary>
        public bool TryPromoteNext(Session session)
        {
            var next = session.Bookings
                .Where(b => b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.CreatedAt)
                .FirstOrDefault();

            if (next is null)
                return false;

            session.PromoteFromWaitlist(next.Id);
            return true;
        }
    }
}
