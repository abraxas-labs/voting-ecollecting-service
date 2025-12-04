// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.DmDoc.Serialization;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.Mocks;

public class CommitteeListTemplateGeneratorMock(IDmDocDataSerializer dmDocDataSerializer, DmDocConfig config)
    : PdfGeneratorMock<CommitteeListTemplateData, CommitteeListTemplateBag>(dmDocDataSerializer), ICommitteeListTemplateGenerator
{
    protected override CommitteeListTemplateBag Map(CommitteeListTemplateData data)
        => DataContainerBuilder.BuildCommitteeListTemplateBag(data);

    protected override string BuildFileName(CommitteeListTemplateData data)
        => config.CommitteeListTemplateFileName;
}
