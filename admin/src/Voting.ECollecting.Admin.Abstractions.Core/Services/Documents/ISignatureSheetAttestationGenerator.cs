// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Core.Models;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;

public interface ISignatureSheetAttestationGenerator : IPdfGenerator<SignatureSheetAttestationTemplateData>;
