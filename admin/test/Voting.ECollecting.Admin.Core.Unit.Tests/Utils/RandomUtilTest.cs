// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Voting.ECollecting.Admin.Core.Utils;

namespace Voting.ECollecting.Admin.Core.Unit.Tests.Utils;

public class RandomUtilTest
{
    [Fact]
    public void ShouldReturn()
    {
        RandomUtil.GenerateReferendumNumber().Length.Should().Be(8);
    }

    [Fact]
    public void ShouldReturnOnlyWithDesiredChars()
    {
        var chars = "10IO".ToCharArray();
        for (var i = 0; i < 100; i++)
        {
            var randomString = RandomUtil.GenerateReferendumNumber();
            foreach (var c in randomString)
            {
                chars.Should().NotContain(c);
            }
        }
    }

    [Fact]
    public void ShouldReturnRandomString()
    {
        var random1 = RandomUtil.GenerateReferendumNumber();
        var random2 = RandomUtil.GenerateReferendumNumber();
        random1.Equals(random2).Should().BeFalse();
    }

    [Fact]
    public void ShouldReturnNoExistingString()
    {
        var existingNumber = RandomUtil.GenerateReferendumNumber();
        var random = RandomUtil.GenerateReferendumNumber(new HashSet<string> { existingNumber });
        random.Equals(existingNumber).Should().BeFalse();
    }
}
