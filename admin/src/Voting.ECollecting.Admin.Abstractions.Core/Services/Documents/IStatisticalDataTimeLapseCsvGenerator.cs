// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Core.Models;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;

public interface IStatisticalDataTimeLapseCsvGenerator
{
    Task<IFile> GenerateFile(StatisticalDataTimeLapseTemplateData data);
}
