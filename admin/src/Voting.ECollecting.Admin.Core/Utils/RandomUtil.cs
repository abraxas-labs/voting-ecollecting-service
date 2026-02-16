// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Security.Cryptography;

namespace Voting.ECollecting.Admin.Core.Utils;

public static class RandomUtil
{
    public static string GenerateSecureIdNumber(IReadOnlySet<string>? existingNumbers = null)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        const int size = 12;
        const int maxIterations = 100;
        for (var i = 0; i < maxIterations; i++)
        {
            var number = RandomNumberGenerator.GetString(chars, size);
            var exists = existingNumbers?.Contains(number);
            if (exists != true)
            {
                return number;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique secure id number after multiple attempts.");
    }
}
