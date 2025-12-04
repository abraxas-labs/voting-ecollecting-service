// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;

public interface IElectronicSignaturesProtocolGenerator : IPdfGenerator<ECollectingProtocolTemplateData>;
