using Microsoft.Extensions.DependencyInjection;
using Slotwise.Application.Abstractions;
using Slotwise.Application.Sessions.Commands.BookSession;
using Slotwise.Application.Sessions.Commands.CancelBooking;
using Slotwise.Application.Sessions.Commands.CreateSession;
using Slotwise.Application.Sessions.DTOs;
using Slotwise.Application.Sessions.Queries.GetSession;
using Slotwise.Application.Sessions.Queries.ListSessions;
using Slotwise.Domain.Sessions.Services;

namespace Slotwise.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<WaitlistPromotionPolicy>();

            services.AddScoped<ICommandHandler<CreateSessionCommand, Guid>, CreateSessionCommandHandler>();
            services.AddScoped<ICommandHandler<BookSessionCommand, BookingDTO>, BookSessionCommandHandler>();
            services.AddScoped<ICommandHandler<CancelBookingCommand>, CancelBookingCommandHandler>();

            services.AddScoped<IQueryHandler<GetSessionQuery, SessionDTO>, GetSessionQueryHandler>();
            services.AddScoped<IQueryHandler<ListSessionsQuery, IReadOnlyList<SessionDTO>>, ListSessionsQueryHandler>();

            return services;
        }
    }
}
