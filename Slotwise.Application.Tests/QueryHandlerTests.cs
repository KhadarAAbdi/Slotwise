using Moq;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Application.Sessions.Queries.GetSession;
using Slotwise.Application.Sessions.Queries.ListSessions;

namespace Slotwise.Application.Tests;

public class QueryHandlerTests
{
    private readonly Mock<ISessionQueries> _queries = new();

    private static SessionDTO NewDto(Guid? id = null)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return new SessionDTO(id ?? Guid.NewGuid(), "Yoga", start, start.AddHours(1), 10, 0, 0, new List<BookingDTO>());
    }

    [Fact]
    public async Task GetSession_Existing_ReturnsTheDto()
    {
        var dto = NewDto();
        _queries.Setup(q => q.GetByIdAsync(dto.Id)).ReturnsAsync(dto);
        var handler = new GetSessionQueryHandler(_queries.Object);

        var result = await handler.Handle(new GetSessionQuery(dto.Id));

        Assert.Same(dto, result);
    }

    [Fact]
    public async Task GetSession_Missing_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _queries.Setup(q => q.GetByIdAsync(id)).ReturnsAsync((SessionDTO?)null);
        var handler = new GetSessionQueryHandler(_queries.Object);

        await Assert.ThrowsAsync<SessionNotFoundException>(() => handler.Handle(new GetSessionQuery(id)));
    }

    [Fact]
    public async Task ListSessions_ReturnsWhatTheQueriesReturn()
    {
        IReadOnlyList<SessionDTO> expected = new List<SessionDTO> { NewDto(), NewDto() };
        _queries.Setup(q => q.ListAsync()).ReturnsAsync(expected);
        var handler = new ListSessionsQueryHandler(_queries.Object);

        var result = await handler.Handle(new ListSessionsQuery());

        Assert.Same(expected, result);
    }
}
