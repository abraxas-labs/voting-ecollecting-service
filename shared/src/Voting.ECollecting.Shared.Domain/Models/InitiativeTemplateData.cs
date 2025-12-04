// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.Models;

public record InitiativeTemplateData(InitiativeEntity Initiative, IEnumerable<InitiativeCommitteeMember> CommitteeMembers);
