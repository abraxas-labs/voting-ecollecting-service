// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Adapter.Data.ModelBuilders;

/// <summary>
/// Database Context Accessor extensions.
/// </summary>
internal static class DbContextAccessor
{
    // Needed to have access to the DataContext inside model builders
    internal static DataContext DbContext { get; set; } = null!;
}
