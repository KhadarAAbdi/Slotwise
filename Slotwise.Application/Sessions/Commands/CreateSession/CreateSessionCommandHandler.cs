using Slotwise.Application.Abstractions;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Sessions.Entities;
using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Application.Sessions.Commands.CreateSession
{
    public class CreateSessionCommandHandler : ICommandHandler<CreateSessionCommand, Guid>
    {
        private readonly ISessionRepository _repository;

        public CreateSessionCommandHandler(ISessionRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreateSessionCommand command)
        {
            TimeSlot sessionTimeslot = new TimeSlot(command.Start, command.End);
            SeatCount seatCount = new SeatCount(command.Capacity);
            Session session = new Session(command.Title, sessionTimeslot, seatCount);

            await _repository.AddAsync(session);
            await _repository.SaveChangesAsync();

            return session.Id;
        }
    }
}
