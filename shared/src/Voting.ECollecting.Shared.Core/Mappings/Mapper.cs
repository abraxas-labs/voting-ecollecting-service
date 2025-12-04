// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Core.Mappings;

[Mapper]
internal static partial class Mapper
{
    [MapperIgnoreTarget(nameof(InitiativeCommitteeMember.Residence))]
    [MapperIgnoreTarget(nameof(InitiativeCommitteeMember.PoliticalResidence))]
    internal static partial InitiativeCommitteeMember MapToInitiativeCommitteeMember(InitiativeCommitteeMemberEntity initiativeCommitteeMember);
}
