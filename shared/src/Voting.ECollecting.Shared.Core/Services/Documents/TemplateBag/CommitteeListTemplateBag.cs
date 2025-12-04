// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;

public record CommitteeListTemplateBag(
    string DomainOfInfluenceType,
    string CollectionType,
    string CollectionDescription,
    int MinimumNumberOfCommitteeMembers,
    List<InitiativeCommitteeMemberDataContainer> CommitteeMembers,
    CollectionPermissionDataContainer CollectionOwner,
    List<CollectionPermissionDataContainer> CollectionDeputies);
