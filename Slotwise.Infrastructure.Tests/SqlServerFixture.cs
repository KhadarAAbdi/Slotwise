using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Slotwise.Infrastructure.Persistence;

namespace Slotwise.Infrastructure.Tests;

/// <summary>
/// Creates one throwaway SQL Server database for the whole test run, applies the real
/// migrations to it (so the migrations and the model are tested too), and drops it afterwards.
/// The server comes from SLOTWISE_TEST_SQLSERVER, defaulting to a local SQL Express instance.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    private const string ServerVariable = "SLOTWISE_TEST_SQLSERVER";
    private const string DefaultServer = @"Server=localhost\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly string _serverConnectionString =
        Environment.GetEnvironmentVariable(ServerVariable) ?? DefaultServer;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var database = "SlotwiseTests_" + Guid.NewGuid().ToString("N");
        ConnectionString = new SqlConnectionStringBuilder(_serverConnectionString) { InitialCatalog = database }.ConnectionString;

        await WaitForServerAsync();

        var context = CreateContext();
        try
        {
            await context.Database.MigrateAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }

    public async Task DisposeAsync()
    {
        var context = CreateContext();

        try
        {
            await context.Database.EnsureDeletedAsync();
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }

    public SlotwiseDbContext CreateContext()
    {
        return new SlotwiseDbContext(new DbContextOptionsBuilder<SlotwiseDbContext>().UseSqlServer(ConnectionString).Options);
    }
        

    private async Task WaitForServerAsync()
    {
        var master = new SqlConnectionStringBuilder(_serverConnectionString) { InitialCatalog = "master" }.ConnectionString;
        var deadline = DateTime.UtcNow.AddSeconds(90);

        while (true)
        {
            try
            {
                await using var connection = new SqlConnection(master);
                await connection.OpenAsync();
                return;
            }
            catch (SqlException) when (DateTime.UtcNow < deadline)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}

[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
