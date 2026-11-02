using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Slotwise.Application.Abstractions;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Domain.Sessions.Events;
using Slotwise.Infrastructure.Events;
using Slotwise.Infrastructure.Persistence;
using Slotwise.Infrastructure.Persistence.Queries;
using Slotwise.Infrastructure.Persistence.Repositories;
using Slotwise.Infrastructure.Projections;

namespace Slotwise.Infrastructure
{
    public static class DependencyInjection
    {
        public const string ConnectionStringName = "SlotwiseDb";

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' is not configured. " +
                    "Set ConnectionStrings:SlotwiseDb in appsettings.json or the ConnectionStrings__SlotwiseDb environment variable.");
            var mongoConnectionString = configuration.GetConnectionString("MongoDb")
                ?? throw new InvalidOperationException(
                    $"Connection string 'MongoDb' is not configured. " +
                    "Set ConnectionStrings:MongoDb in appsettings.json or the ConnectionStrings__MongoDb environment variable.");
            var mongoDatabaseName = MongoUrl.Create(mongoConnectionString).DatabaseName
                ?? throw new InvalidOperationException(
                    "Connection string 'MongoDb' must name a database, e.g. mongodb://localhost:27017/slotwise."); 

            services.AddDbContext<SlotwiseDbContext>(options => options.UseSqlServer(connectionString));
            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
            services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<ISessionQueries, SessionQueries>();

            // The projection handler reads SQL directly, so the SQL queries class is also registered as itself:
            // ISessionQueries is about to point at Mongo, and the handler must keep reading the source of truth.
            services.AddScoped<SessionQueries>();

            // SpotOpened is left out on purpose: it is always raised together with BookingCancelled,
            // which already refreshes the document.
            services.AddScoped<IDomainEventHandler<SessionCreated>, SessionProjectionHandler<SessionCreated>>();
            services.AddScoped<IDomainEventHandler<BookingConfirmed>, SessionProjectionHandler<BookingConfirmed>>();
            services.AddScoped<IDomainEventHandler<BookingWaitlisted>, SessionProjectionHandler<BookingWaitlisted>>();
            services.AddScoped<IDomainEventHandler<BookingCancelled>, SessionProjectionHandler<BookingCancelled>>();
            services.AddScoped<IDomainEventHandler<WaitlistPromoted>, SessionProjectionHandler<WaitlistPromoted>>();
            return services;
        }
    }
}
