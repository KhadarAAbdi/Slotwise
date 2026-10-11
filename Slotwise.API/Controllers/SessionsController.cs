using Microsoft.AspNetCore.Mvc;
using Slotwise.API.Contracts;
using Slotwise.Application.Abstractions;
using Slotwise.Application.Sessions.Commands.BookSession;
using Slotwise.Application.Sessions.Commands.CancelBooking;
using Slotwise.Application.Sessions.Commands.CreateSession;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Application.Sessions.Queries.GetSession;
using Slotwise.Application.Sessions.Queries.ListSessions;

namespace Slotwise.API.Controllers
{
    [ApiController]
    [Route("sessions")]
    public class SessionsController : ControllerBase
    {
        private readonly ICommandHandler<CreateSessionCommand, Guid> _createSession;
        private readonly ICommandHandler<BookSessionCommand, BookingDTO> _bookSession;
        private readonly ICommandHandler<CancelBookingCommand> _cancelBooking;
        private readonly IQueryHandler<GetSessionQuery, SessionDTO> _getSession;
        private readonly IQueryHandler<ListSessionsQuery, IReadOnlyList<SessionDTO>> _listSessions;

        public SessionsController(
            ICommandHandler<CreateSessionCommand, Guid> createSession,
            ICommandHandler<BookSessionCommand, BookingDTO> bookSession,
            ICommandHandler<CancelBookingCommand> cancelBooking,
            IQueryHandler<GetSessionQuery, SessionDTO> getSession,
            IQueryHandler<ListSessionsQuery, IReadOnlyList<SessionDTO>> listSessions)
        {
            _createSession = createSession;
            _bookSession = bookSession;
            _cancelBooking = cancelBooking;
            _getSession = getSession;
            _listSessions = listSessions;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSessionRequest request)
        {
            var id = await _createSession.Handle(
                new CreateSessionCommand(request.Title, request.Start, request.End, request.Capacity));

            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SessionDTO>>> List()
        {
            return Ok(await _listSessions.Handle(new ListSessionsQuery()));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SessionDTO>> Get(Guid id)
        {
            return Ok(await _getSession.Handle(new GetSessionQuery(id)));
        }

        [HttpPost("{id:guid}/bookings")]
        public async Task<ActionResult<BookingDTO>> Book(Guid id, BookSessionRequest request)
        {
            var booking = await _bookSession.Handle(new BookSessionCommand(id, request.Email));

            return Created($"/sessions/{id}", booking);
        }

        [HttpDelete("{id:guid}/bookings/{bookingId:guid}")]
        public async Task<IActionResult> CancelBooking(Guid id, Guid bookingId)
        {
            await _cancelBooking.Handle(new CancelBookingCommand(id, bookingId));

            return NoContent();
        }
    }
}
