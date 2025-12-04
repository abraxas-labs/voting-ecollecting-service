// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Test.Utils;

public static class DatabaseUtil
{
    private static bool _migrated;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Referencing hardened interpolated string parameters.")]
    public static void Truncate(params DbContext[] dbContexts)
    {
        // on the first run, we migrate the database to ensure the same structure as the "real" DB
        if (!_migrated)
        {
            foreach (var db in dbContexts)
            {
                db.Database.Migrate();
            }

            _migrated = true;
        }

        foreach (var db in dbContexts)
        {
            // truncating tables is much faster than recreating the database
            var tableNames = db.Model.GetEntityTypes()

                // ignore InitiativeSubTypeEntity since this is seeded via HasData and should never be modified by the tests
                .Where(x => x.ClrType != typeof(InitiativeSubTypeEntity))
                .Select(m => $@"""{m.GetTableName()}""")
                .Distinct();
            db.Database.ExecuteSqlRaw($"TRUNCATE {string.Join(",", tableNames)} CASCADE");
        }
    }
}
