// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public record SecondFactorTransactionInfo(
    Guid Id,
    string CorrelationCode,
    string QrCode);
