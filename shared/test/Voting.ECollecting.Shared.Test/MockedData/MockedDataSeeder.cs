// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.Context;
using Voting.ECollecting.Shared.Migrations;
using Voting.Lib.Cryptography;

namespace Voting.ECollecting.Shared.Test.MockedData;

public static class MockedDataSeeder
{
    public static Task Seed(
        Func<Func<IServiceProvider, Task>, Task> runScoped,
        SeederArgs? args = null,
        SeederContext? ctx = null)
    {
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<MigrationDataContext>();
            var crypto = sp.GetRequiredService<ICryptoProvider>();
            await Seeder.Seed(
                db,
                crypto,
                args ?? SeederArgs.Default,
                ctx ?? MockedDataSeederContext.Default);
        });
    }
}
