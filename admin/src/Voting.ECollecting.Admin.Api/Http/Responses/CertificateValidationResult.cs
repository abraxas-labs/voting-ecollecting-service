// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;

namespace Voting.ECollecting.Admin.Api.Http.Responses;

public record CertificateValidationResult(CertificateValidation Validation, CertificateValidationState State);
