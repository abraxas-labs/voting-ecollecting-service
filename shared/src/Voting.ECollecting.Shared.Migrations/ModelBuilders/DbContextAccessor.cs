// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Migrations.ModelBuilders;

internal static class DbContextAccessor
{
    internal static MigrationDataContext DbContext { get; set; } = null!;
}
