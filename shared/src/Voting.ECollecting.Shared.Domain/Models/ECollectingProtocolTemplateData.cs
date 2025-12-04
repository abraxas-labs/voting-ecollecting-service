// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.Models;

public record ECollectingProtocolTemplateData(CollectionBaseEntity Collection, AccessControlListDoiEntity AccessControlListDoi, InitiativeSubTypeEntity? SubType);
