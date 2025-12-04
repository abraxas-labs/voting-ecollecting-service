// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.DataSeeder.Data.Context;

namespace Voting.ECollecting.Shared.Test.MockedData;

public static class MockedTenantContext
{
    public static readonly TenantContext Default = new()
    {
        Bund = MockedTenantIds.KTSG,
        CantonStGallen = MockedTenantIds.KTSG,
        MunicipalityStGallen = MockedTenantIds.MUSG,
        MunicipalityGoldach = MockedTenantIds.MUGOLDACH,
    };
}
