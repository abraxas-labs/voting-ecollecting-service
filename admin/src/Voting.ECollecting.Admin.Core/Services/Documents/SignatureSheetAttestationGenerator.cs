// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.IO;
using Voting.ECollecting.Admin.Abstractions.Core.Models;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Admin.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services.Documents;
using Voting.Lib.DmDoc;

namespace Voting.ECollecting.Admin.Core.Services.Documents;

public class SignatureSheetAttestationGenerator : PdfGenerator<SignatureSheetAttestationTemplateData, SignatureSheetAttestationTemplateBag>, ISignatureSheetAttestationGenerator
{
    private readonly DmDocConfig _config;

    public SignatureSheetAttestationGenerator(
        DmDocConfig config,
        RecyclableMemoryStreamManager memoryStreamManager,
        IDmDocService dmDoc,
        TimeProvider timeProvider)
        : base(config.TemplateKeys.SignatureSheetAttestation, memoryStreamManager, dmDoc, timeProvider, config)
    {
        _config = config;
    }

    protected override SignatureSheetAttestationTemplateBag Map(SignatureSheetAttestationTemplateData entity)
        => TemplateBagMapper.MapToSignatureSheetAttestationTemplateBag(entity);

    protected override string BuildFileName(SignatureSheetAttestationTemplateData entity)
        => AppendTimestampSuffix(_config.SignatureSheetAttestationFileName);
}
