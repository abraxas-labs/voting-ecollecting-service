// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using FluentAssertions;
using Voting.ECollecting.Shared.Core.Services;
using Xunit;

namespace Voting.ECollecting.Shared.Core.Unit.Tests.Services;

public class UserNotificationRendererTest
{
    [Theory]
    [InlineData("hello.world", "hello<span>.</span>world")]
    [InlineData("user@example.com", "user<span>@</span>example<span>.</span>com")]
    [InlineData("http://test.com", "http<span>:</span>//test<span>.</span>com")]
    [InlineData("<script>", "&lt;script&gt;")]
    [InlineData("normal text", "normal text")]
    public void EncodeHtmlShouldEncodeSpecialCharacters(string input, string expectedFragment)
    {
        var result = UserNotificationRenderer.EncodeHtml(input);
        result.Should().Contain(expectedFragment);
    }

    [Theory]
    [InlineData("javascript:alert('x');", false)]
    [InlineData("my-scheme://foo/bar", false)]
    [InlineData("ftp://foo/bar", false)]
#if DEBUG
    [InlineData("https://example.com", false)]
    [InlineData("http://example.com", true)]
#else
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com", false)]
#endif
    public void EncodeHrefShouldAcceptOnlyAllowedScheme(string url, bool valid)
    {
        var act = () => UserNotificationRenderer.EncodeHref(url);
        if (valid)
        {
            act.Should().NotThrow();
            var result = act();
            result.Should().Be(WebUtility.HtmlEncode(url));
        }
        else
        {
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Only https links are allowed in href attributes.");
        }
    }
}
