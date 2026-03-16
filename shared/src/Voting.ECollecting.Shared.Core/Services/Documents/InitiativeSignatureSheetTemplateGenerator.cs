// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.IO;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.DmDoc;

namespace Voting.ECollecting.Shared.Core.Services.Documents;

public class InitiativeSignatureSheetTemplateGenerator : PdfGenerator<InitiativeTemplateData, InitiativeSignatureSheetTemplateBag>, IInitiativeSignatureSheetTemplateGenerator
{
    private readonly DmDocConfig _config;

    public InitiativeSignatureSheetTemplateGenerator(
        DmDocConfig config,
        RecyclableMemoryStreamManager memoryStreamManager,
        IDmDocService dmDoc,
        TimeProvider timeProvider)
        : base(config.TemplateKeys.InitiativeSignatureSheetTemplate, memoryStreamManager, dmDoc, timeProvider, config)
    {
        _config = config;
    }

    protected override InitiativeSignatureSheetTemplateBag Map(InitiativeTemplateData entity) => TemplateBagMapper.MapToInitiativeSignatureSheetTemplateBag(entity);

    protected override string BuildFileName(InitiativeTemplateData templateData)
        => AppendTimestampSuffix(string.Format(_config.SignatureSheetTemplateFileName, templateData.Initiative.Description));
}
