using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Slotwise.Application.Sessions.Interfaces;
using Slotwise.Infrastructure.Persistence;
using Slotwise.Infrastructure.Persistence.Queries;
using Slotwise.Infrastructure.Persistence.Repositories;

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

            services.AddDbContext<SlotwiseDbContext>(options => options.UseSqlServer(connectionString));
            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<ISessionQueries, SessionQueries>();
            return services;
        }
    }
}
