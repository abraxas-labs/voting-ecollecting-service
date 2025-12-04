// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services;

public interface IInitiativeCommitteeMemberService
{
    InitiativeCommitteeMember EnrichCommitteeMember(
        InitiativeCommitteeMemberEntity memberEntity,
        Dictionary<string, AccessControlListDoiEntity> domainOfInfluencesByBfs);
}
