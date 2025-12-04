// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services.Signature;

public interface IInitiativeSignService
{
    Task Sign(Guid id);
}
