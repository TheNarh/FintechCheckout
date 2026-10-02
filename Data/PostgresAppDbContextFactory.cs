using FintechCheckout.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FintechCheckout.Data;

public class PostgresAppDbContextFactory
    : IDesignTimeDbContextFactory<PostgresAppDbContext>
{
    public PostgresAppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            $"Host={configuration["PGHOST"]};" +
            $"Port={configuration["PGPORT"]};" +
            $"Database={configuration["PGDATABASE"]};" +
            $"Username={configuration["PGUSER"]};" +
            $"Password={configuration["PGPASSWORD"]};" +
            $"SSL Mode=Require;" +
            $"Trust Server Certificate=true";

        var optionsBuilder =
            new DbContextOptionsBuilder<PostgresAppDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new PostgresAppDbContext(optionsBuilder.Options);
    }
}