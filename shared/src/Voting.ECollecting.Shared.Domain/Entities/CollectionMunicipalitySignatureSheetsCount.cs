// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities;

public record CollectionMunicipalitySignatureSheetsCount(
    int TotalSignatureSheetsCount,
    int TotalSubmittedOrConfirmedSignatureSheetsCount,
    int TotalNotSubmittedSignatureSheetsCount,
    int TotalConfirmedSignatureSheetsCount);
