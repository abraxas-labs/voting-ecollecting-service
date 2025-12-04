// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.DmDoc.Serialization;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.Mocks;

public class ReferendumSignatureSheetTemplateGeneratorMock(IDmDocDataSerializer dmDocDataSerializer, DmDocConfig config)
    : PdfGeneratorMock<ReferendumEntity, ReferendumSignatureSheetTemplateBag>(dmDocDataSerializer), IReferendumSignatureSheetTemplateGenerator
{
    protected override ReferendumSignatureSheetTemplateBag Map(ReferendumEntity entity)
        => TemplateBagMapper.MapToReferendumSignatureSheetTemplateBag(entity);

    protected override string BuildFileName(ReferendumEntity entity)
        => "Referendum_" + config.SignatureSheetTemplateFileName;
}
