using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slotwise.Infrastructure.Persistence;

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

            services.AddDbContext<SlotwiseDbContext>(options => options.UseSqlServer(connectionString));

            return services;
        }
    }
}
