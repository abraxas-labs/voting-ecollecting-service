// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.Admin;

namespace Voting.ECollecting.Citizen.Adapter.Admin;

public class AdminAdapterMock : IAdminAdapter
{
    public Task NotifyPreparingForCollection() => Task.CompletedTask;
}
