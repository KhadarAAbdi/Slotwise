using Slotwise.Application.Abstractions;
using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Sessions.Events;
using Slotwise.Domain.Sessions.Services;

namespace Slotwise.Application.Sessions.Commands.CancelBooking
{
    public class CancelBookingCommandHandler : ICommandHandler<CancelBookingCommand>
    {
        private readonly ISessionRepository _repository;
        private readonly WaitlistPromotionPolicy _promotionPolicy;

        public CancelBookingCommandHandler(ISessionRepository repository, WaitlistPromotionPolicy promotionPolicy)
        {
            _repository = repository;
            _promotionPolicy = promotionPolicy;
        }

        public async Task Handle(CancelBookingCommand command)
        {
            var session = await _repository.GetByIdAsync(command.SessionId)
                ?? throw new SessionNotFoundException(command.SessionId);

            session.CancelBooking(command.BookingId);

            // Only promote when a confirmed seat actually opened; cancelling a waitlisted
            // booking frees nothing and PromoteFromWaitlist would throw for lack of capacity.
            // This inline call moves to the SpotOpened consumer once messaging exists.
            if (session.DomainEvents.OfType<SpotOpened>().Any())
                _promotionPolicy.TryPromoteNext(session);

            await _repository.SaveChangesAsync();

        }
    }
}
