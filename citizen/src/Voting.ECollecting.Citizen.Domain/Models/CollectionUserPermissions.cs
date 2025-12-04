// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Domain.Models;

public record CollectionUserPermissions(
    CollectionPermissionRole Role,
    bool CanEdit,
    bool CanEditSignatureSheetTemplate,
    bool CanDeleteSignatureSheetTemplate,
    bool CanGenerateSignatureSheetTemplatePreview,
    bool CanEditPermissions,
    bool CanCreateMessages,
    bool CanSubmit,
    bool IsSubmitVisible,
    bool CanFlagForReview,
    bool IsFlagForReviewVisible,
    bool CanRegister,
    bool CanWithdraw,
    bool CanRequestInformalReview,
    bool IsRequestInformalReviewVisible,
    bool CanDownloadElectronicSignaturesProtocol,
    bool CanEditSubType)
{
    public static readonly CollectionUserPermissions ReadOnly = new(
        CollectionPermissionRole.Reader,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false);
}
