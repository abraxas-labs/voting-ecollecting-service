// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Shared.Core.Services;

public abstract class UserNotificationRenderer : IUserNotificationRenderer
{
#if DEBUG
    private static readonly string _allowedHrefScheme = Uri.UriSchemeHttp;
#else
    private static readonly string _allowedHrefScheme = Uri.UriSchemeHttps;
#endif

    [return: NotNullIfNotNull(nameof(input))]
    public static string? EncodeHtml(string? input)
    {
        if (input == null)
        {
            return null;
        }

        var encoded = WebUtility.HtmlEncode(input);

        // By replacing these characters, we fool email parses such as Gmail to not parse the highlight the texts as
        // links, to prevent phishing.
        return encoded
            .Replace(".", "<span>.</span>")
            .Replace("@", "<span>@</span>")
            .Replace(":", "<span>:</span>");
    }

    public static string EncodeHref(string url)
    {
        var encoded = WebUtility.HtmlEncode(url);
        if (!Uri.TryCreate(encoded, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, _allowedHrefScheme, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Only https links are allowed in href attributes.");
        }

        return encoded;
    }

    public UserNotification Render(UserNotificationEntity notification)
    {
        var subject = RenderSubject(notification.TemplateBag).Truncate(100);
        return new UserNotification(
            notification.RecipientEMail,
            subject,
            ContainerHtml(subject, RenderBodyHtml(notification.TemplateBag)));
    }

    protected static string Html([StringSyntax("html")] string html) => html;

    protected abstract string RenderSubject(UserNotificationTemplateBag templateBag);

    protected abstract string RenderBodyHtml(UserNotificationTemplateBag templateBag);

    private static string ContainerHtml(string title, [StringSyntax("html")] string bodyHtml)
    {
        return Html($$"""
                      <!DOCTYPE html>
                      <html lang="de-CH">
                      <head>
                        <meta charset="UTF-8" />
                        <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
                        <title>{{WebUtility.HtmlEncode(title)}}</title>
                        <style>
                          body {
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            margin: 0;
                            padding: 0;
                          }
                          .container {
                            background-color: #ffffff;
                            max-width: 600px;
                            margin: 40px auto;
                            padding: 20px;
                            border-radius: 6px;
                            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                          }
                          .user-text {
                            hyphens: auto;
                            overflow-wrap: break-word;
                            white-space: pre-wrap;
                          }
                        </style>
                      </head>
                      <body>
                        <div class="container">{{bodyHtml}}</div>
                      </body>
                      </html>
                      """);
    }
}
