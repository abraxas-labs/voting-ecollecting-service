// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Database.Migrations;

namespace Voting.ECollecting.Shared.Migrations.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseMigrationServices(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
    {
        return services
            .AddSingleton<IDatabaseMigrator, DatabaseMigrator<MigrationDataContext>>()
            .AddDbContext<MigrationDataContext>(optionsBuilder);
    }
}
