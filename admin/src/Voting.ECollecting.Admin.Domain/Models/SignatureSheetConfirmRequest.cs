// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Domain.Models;

public record SignatureSheetConfirmRequest(
    Guid CollectionId,
    Guid SheetId,
    CollectionType CollectionType,
    IReadOnlySet<Guid> AddedPersonRegisterIds,
    IReadOnlySet<Guid> RemovedPersonRegisterIds,
    int SignatureCountTotal);
