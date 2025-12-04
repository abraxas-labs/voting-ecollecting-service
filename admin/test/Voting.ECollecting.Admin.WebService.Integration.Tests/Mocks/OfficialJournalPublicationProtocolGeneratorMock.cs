// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.DmDoc.Serialization;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;

public class OfficialJournalPublicationProtocolGeneratorMock(IDmDocDataSerializer dmDocDataSerializer, DmDocConfig config)
    : PdfGeneratorMock<ECollectingProtocolTemplateData, ECollectingProtocolDataContainer>(dmDocDataSerializer), IOfficialJournalPublicationProtocolGenerator
{
    protected override ECollectingProtocolDataContainer Map(ECollectingProtocolTemplateData data)
        => DataContainerBuilder.BuildProtocolDataContainer(data);

    protected override string BuildFileName(ECollectingProtocolTemplateData data)
        => $"{data.Collection.Description}_{config.OfficialJournalPublicationProtocolFileName}";
}
