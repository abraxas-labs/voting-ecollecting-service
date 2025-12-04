// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;

public interface IStatisticalDataCsvGenerator
{
    IFile GenerateFile(CollectionBaseEntity collection);
}
