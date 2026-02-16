// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Core.Mappings;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Core.Services;

public class InitiativeCommitteeMemberService : IInitiativeCommitteeMemberService
{
    public InitiativeCommitteeMember EnrichCommitteeMember(InitiativeCommitteeMemberEntity memberEntity, Dictionary<string, AccessControlListDoiEntity> domainOfInfluencesByBfs)
    {
        var member = Mapper.MapToInitiativeCommitteeMember(memberEntity);
        member.Residence = domainOfInfluencesByBfs.GetValueOrDefault(memberEntity.Bfs)?.Name ?? string.Empty;
        return member;
    }
}
