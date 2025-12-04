// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.DmDoc.Serialization;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;

public class InitiativeSignatureSheetTemplateGeneratorMock(IDmDocDataSerializer dmDocDataSerializer, DmDocConfig config)
    : PdfGeneratorMock<InitiativeTemplateData, InitiativeSignatureSheetTemplateBag>(dmDocDataSerializer), IInitiativeSignatureSheetTemplateGenerator
{
    protected override InitiativeSignatureSheetTemplateBag Map(InitiativeTemplateData entity)
        => TemplateBagMapper.MapToInitiativeSignatureSheetTemplateBag(entity);

    protected override string BuildFileName(InitiativeTemplateData entity)
        => "Initiative_" + config.SignatureSheetTemplateFileName;
}
