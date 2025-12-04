// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Domain.Models;

public class CollectionSignatureSheet : CollectionSignatureSheetEntity
{
    public CollectionSignatureSheetUserPermissions? UserPermissions { get; set; }
}
