// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Core.Models;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Admin.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.Lib.DmDoc.Serialization;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;

public class SignatureSheetAttestationGeneratorMock(IDmDocDataSerializer dmDocDataSerializer, DmDocConfig config)
    : PdfGeneratorMock<SignatureSheetAttestationTemplateData, SignatureSheetAttestationTemplateBag>(dmDocDataSerializer), ISignatureSheetAttestationGenerator
{
    protected override SignatureSheetAttestationTemplateBag Map(SignatureSheetAttestationTemplateData entity)
        => TemplateBagMapper.MapToSignatureSheetAttestationTemplateBag(entity);

    protected override string BuildFileName(SignatureSheetAttestationTemplateData entity)
        => config.SignatureSheetAttestationFileName;
}
