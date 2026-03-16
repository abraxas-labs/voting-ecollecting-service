// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.IO;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.DmDoc;

namespace Voting.ECollecting.Shared.Core.Services.Documents;

public class CommitteeListTemplateGenerator : PdfGenerator<CommitteeListTemplateData, CommitteeListTemplateBag>,
    ICommitteeListTemplateGenerator
{
    private readonly DmDocConfig _config;

    public CommitteeListTemplateGenerator(
        DmDocConfig config,
        RecyclableMemoryStreamManager memoryStreamManager,
        IDmDocService dmDoc,
        TimeProvider timeProvider)
        : base(config.TemplateKeys.CommitteeList, memoryStreamManager, dmDoc, timeProvider, config)
    {
        _config = config;
    }

    protected override CommitteeListTemplateBag Map(CommitteeListTemplateData data)
        => DataContainerBuilder.BuildCommitteeListTemplateBag(data);

    protected override string BuildFileName(CommitteeListTemplateData data)
        => AppendTimestampSuffix(_config.CommitteeListTemplateFileName);
}
