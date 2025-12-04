// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace Voting.ECollecting.Shared.Migrations;

/// <summary>
/// Data context factory used to create ef core migrations.
/// This class is compiled only for development configuration and will be removed for 'Release' configuration.
/// </summary>
internal class DataContextFactory : IDesignTimeDbContextFactory<MigrationDataContext>
{
    /// <summary>
    /// Gets a dummy connection string to create ef core migrations.
    /// </summary>
    private string ConnectionString => new NpgsqlConnectionStringBuilder
    {
        Host = "localhost",
        Username = "user",
        Password = DummyPass,
        Database = "voting-ecollecting",
    }.ToString();

    private string DummyPass => "password";

    public MigrationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MigrationDataContext>();
        optionsBuilder.UseNpgsql(ConnectionString);

        return new MigrationDataContext(optionsBuilder.Options);
    }
}
