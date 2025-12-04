// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Security.Cryptography;

namespace Voting.ECollecting.Admin.Core.Utils;

public static class RandomUtil
{
    public static string GenerateReferendumNumber(IReadOnlySet<string>? existingNumbers = null)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        const int size = 8;
        const int maxIterations = 100;
        for (var i = 0; i < maxIterations; i++)
        {
            var number = RandomNumberGenerator.GetString(chars.ToCharArray(), size);
            var exists = existingNumbers?.Any(x => x == number);
            if (exists != true)
            {
                return number;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique referendum number after multiple attempts.");
    }
}
