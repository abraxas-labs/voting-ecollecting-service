// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;

internal static class ProtoMockedClock
{
    public static Date ToProtoDate(this DateTime date)
    {
        return new Date { Year = date.Year, Month = date.Month, Day = date.Day, };
    }

    public static DateOnly ToDate(this Date date)
        => new DateOnly(date.Year, date.Month, date.Day);
}
