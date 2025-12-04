// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.DataSeeder.Data.Context;
using Voting.Lib.Testing;

namespace Voting.ECollecting.Shared.Test.MockedData;

public static class MockedUserContext
{
    public static readonly UserContext Default = new()
    {
        Admin = new()
        {
            Id = TestDefaults.UserId,
            Name = "Test Admin",
            EMail = "admin@example.com",
        },

        CitizenCreator = new()
        {
            Id = TestDefaults.UserId,
            Name = "Test Citizen Creator",
            EMail = "citizen-creator@example.com",
        },

        CitizenDeputy = new()
        {
            Id = CitizenAuthMockDefaults.DeputyUserId,
            Name = "Test Citizen Deputy",
            EMail = "citizen-deputy@example.com",
        },

        CitizenDeputyNotAccepted = new()
        {
            Id = CitizenAuthMockDefaults.DeputyNotAcceptedUserId,
            Name = "Test Citizen Deputy (not accepted)",
            EMail = "citizen-deputy-not-accepted@example.com",
        },

        CitizenReader = new()
        {
            Id = CitizenAuthMockDefaults.ReaderUserId,
            Name = "Test Citizen Reader",
            EMail = "citizen-reader@example.com",
        },

        CitizenReaderNotAccepted = new()
        {
            Id = CitizenAuthMockDefaults.ReaderNotAcceptedUserId,
            Name = "Test Citizen Reader (not accepted)",
            EMail = "citizen-reader-not-accepted@example.com",
        },
    };
}
