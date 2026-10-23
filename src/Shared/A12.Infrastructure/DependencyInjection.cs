using A12.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A12.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "DefaultConnection";

    public static IServiceCollection AddA12Database(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is missing from web.config.");
        }

        services.AddDbContext<A12DbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }

    public static WebApplication MapA12DatabaseHealth(this WebApplication app)
    {
        app.MapGet("/health/db", async (A12DbContext db, CancellationToken cancellationToken) =>
        {
            try
            {
                var connection = db.Database.GetDbConnection();
                await connection.OpenAsync(cancellationToken);
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT
                        CAST(@@SERVERNAME AS nvarchar(128)),
                        CAST(DB_NAME() AS nvarchar(128)),
                        CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE') AS int)
                    """;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    return Results.Json(
                        new { status = "Failed", error = "The database did not return a status row." },
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }

                return Results.Ok(new
                {
                    status = "Connected",
                    server = reader.GetString(0),
                    database = reader.GetString(1),
                    tableCount = reader.GetInt32(2)
                });
            }
            catch (Exception ex)
            {
                return Results.Json(
                    new { status = "Failed", error = ex.Message },
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        });

        return app;
    }
}
