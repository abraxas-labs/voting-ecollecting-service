// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Core.Services.Documents;

public record StatisticalDataTimeLapseAggregateData(DateOnly Date, string MunicipalityName, int ElectronicCitizenCount, int PhysicalValidCount, int PhysicalInvalidCount);
