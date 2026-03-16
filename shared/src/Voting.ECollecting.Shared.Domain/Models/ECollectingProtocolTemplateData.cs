// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Models;

public record ECollectingProtocolTemplateData(List<CollectionBaseEntity> Collections, DomainOfInfluenceEntity DomainOfInfluence, string Description, DomainOfInfluenceType DomainOfInfluenceType, bool IsDecree, InitiativeSubTypeEntity? SubType = null);
