// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.DataSeeder.Data.Context;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Shared.Test.MockedData;

public static class MockedDataSeederContext
{
    public static readonly SeederContext Default = new(
        MockedUserContext.Default,
        MockedTenantContext.Default,
        MockedClock.UtcNowDate);
}
