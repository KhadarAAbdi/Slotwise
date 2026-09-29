using Slotwise.Application.Sessions.DTOs;

namespace Slotwise.Application.Tests;

public class DtoTests
{
    [Fact]
    public void BookingDto_ExposesItsValuesAndHasValueEquality()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var dto = new BookingDTO(id, "a@example.com", "Confirmed", createdAt);

        Assert.Equal(id, dto.Id);
        Assert.Equal("a@example.com", dto.Email);
        Assert.Equal("Confirmed", dto.Status);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Equal(dto, dto with { });
        Assert.Equal(dto.GetHashCode(), (dto with { }).GetHashCode());
        Assert.Contains("a@example.com", dto.ToString());
    }

    [Fact]
    public void SessionDto_ExposesItsValues()
    {
        var id = Guid.NewGuid();
        var start = DateTime.UtcNow.AddDays(1);
        var bookings = new List<BookingDTO> { new(Guid.NewGuid(), "a@example.com", "Confirmed", DateTime.UtcNow) };

        var dto = new SessionDTO(id, "Yoga", start, start.AddHours(1), 10, 1, 2, bookings);

        Assert.Equal(id, dto.Id);
        Assert.Equal("Yoga", dto.Title);
        Assert.Equal(start, dto.Start);
        Assert.Equal(start.AddHours(1), dto.End);
        Assert.Equal(10, dto.Capacity);
        Assert.Equal(1, dto.ConfirmedCount);
        Assert.Equal(2, dto.WaitlistCount);
        Assert.Same(bookings, dto.Bookings);
        Assert.Equal(dto, dto with { });
        Assert.Equal(dto.GetHashCode(), (dto with { }).GetHashCode());
        Assert.Contains("Yoga", dto.ToString());
    }
}
