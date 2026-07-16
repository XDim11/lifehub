using System.Data.Common;
using LifeHub.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace LifeHub.Api.IntegrationTests.Infrastructure;

public sealed class LifeHubApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                descriptor =>
                    descriptor.ServiceType
                    == typeof(
                        IDbContextOptionsConfiguration<LifeHubDbContext>
                    )
            );

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbConnectionDescriptor = services.SingleOrDefault(
                descriptor =>
                    descriptor.ServiceType == typeof(DbConnection)
            );

            if (dbConnectionDescriptor is not null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection(
                    "Data Source=:memory:"
                );

                connection.Open();

                return connection;
            });

            services.AddDbContext<LifeHubDbContext>(
                (serviceProvider, options) =>
                {
                    var connection =
                        serviceProvider.GetRequiredService<DbConnection>();

                    options.UseSqlite(connection);
                }
            );
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<LifeHubDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}