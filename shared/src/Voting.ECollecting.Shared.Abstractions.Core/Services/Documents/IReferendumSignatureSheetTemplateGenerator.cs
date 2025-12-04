// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;

public interface IReferendumSignatureSheetTemplateGenerator : IPdfGenerator<ReferendumEntity>;
