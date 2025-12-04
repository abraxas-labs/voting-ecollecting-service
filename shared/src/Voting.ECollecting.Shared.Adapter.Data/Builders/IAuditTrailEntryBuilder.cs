// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Adapter.Data.Models;

namespace Voting.ECollecting.Shared.Adapter.Data.Builders;

public interface IAuditTrailEntryBuilder
{
    AuditTrailEntryBuilderResult BuildAuditTrailEntries(DbContext dbContext);
}
