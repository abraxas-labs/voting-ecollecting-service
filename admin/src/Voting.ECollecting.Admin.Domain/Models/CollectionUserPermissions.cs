// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public record CollectionUserPermissions(
    bool CanEdit,
    bool IsRequestInformalReviewVisible,
    bool CanCreateMessages,
    bool CanFinishCorrection,
    bool CanSetCollectionPeriod,
    bool CanEnable,
    bool CanDelete,
    bool CanDeleteWithdrawn,
    bool CanReadTotalCount,
    bool CanAddSignatureSheet,
    bool CanReadSignatureSheets,
    bool CanCheckSamples,
    bool CanSubmitSignatureSheets,
    bool CanFinish,
    bool CanGenerateDocuments,
    bool CanEditAdmissibilityDecision,
    bool CanDeleteAdmissibilityDecision,
    bool CanEditGeneralInformation,
    bool CanReturnForCorrection);
