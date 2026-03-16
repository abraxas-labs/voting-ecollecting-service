// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.IO;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.DmDoc;

namespace Voting.ECollecting.Shared.Core.Services.Documents;

public class OfficialJournalPublicationProtocolGenerator : PdfGenerator<ECollectingProtocolTemplateData, ECollectingProtocolDataContainer>,
    IOfficialJournalPublicationProtocolGenerator
{
    private readonly DmDocConfig _config;

    public OfficialJournalPublicationProtocolGenerator(
        DmDocConfig config,
        RecyclableMemoryStreamManager memoryStreamManager,
        IDmDocService dmDoc,
        TimeProvider timeProvider)
        : base(config.TemplateKeys.OfficialJournalPublicationProtocol, memoryStreamManager, dmDoc, timeProvider, config)
    {
        _config = config;
    }

    protected override ECollectingProtocolDataContainer Map(ECollectingProtocolTemplateData data) =>
            DataContainerBuilder.BuildProtocolDataContainer(data);

    protected override string BuildFileName(ECollectingProtocolTemplateData data)
        => AppendTimestampSuffix($"{data.Description}_{_config.OfficialJournalPublicationProtocolFileName}");
}
