using Slotwise.Application.Abstractions;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Application.Sessions.Commands.BookSession
{
    public class BookSessionCommandHandler : ICommandHandler<BookSessionCommand, BookingDTO>
    {
        private readonly ISessionRepository _repository;

        public BookSessionCommandHandler(ISessionRepository repository)
        {
            _repository = repository;
        }

        public async Task<BookingDTO> Handle(BookSessionCommand command)
        {
            var session = await _repository.GetByIdAsync(command.SessionId)
                ?? throw new SessionNotFoundException(command.SessionId);

            var booking = session.Book(new EmailAddress(command.Email));

            await _repository.SaveChangesAsync();

            return new BookingDTO(booking.Id, booking.EmailAddress.Value, booking.Status.ToString(), booking.CreatedAt);
        }
    }
}
