// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public record CollectionSignatureSheetUserPermissions(
    bool CanEdit,
    bool CanDelete,
    bool CanSubmit,
    bool CanUnsubmit,
    bool CanDiscard,
    bool CanRestore,
    bool CanConfirm);
