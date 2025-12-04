// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.IO;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.DmDoc;

namespace Voting.ECollecting.Shared.Core.Services.Documents;

public class ReferendumSignatureSheetTemplateGenerator : PdfGenerator<ReferendumEntity, ReferendumSignatureSheetTemplateBag>, IReferendumSignatureSheetTemplateGenerator
{
    private readonly DmDocConfig _config;

    public ReferendumSignatureSheetTemplateGenerator(
        DmDocConfig config,
        RecyclableMemoryStreamManager memoryStreamManager,
        IDmDocService dmDoc)
        : base(config.TemplateKeys.ReferendumSignatureSheet, memoryStreamManager, dmDoc)
    {
        _config = config;
    }

    protected override ReferendumSignatureSheetTemplateBag Map(ReferendumEntity entity) => TemplateBagMapper.MapToReferendumSignatureSheetTemplateBag(entity);

    protected override string BuildFileName(ReferendumEntity fileName) => _config.SignatureSheetTemplateFileName;
}
