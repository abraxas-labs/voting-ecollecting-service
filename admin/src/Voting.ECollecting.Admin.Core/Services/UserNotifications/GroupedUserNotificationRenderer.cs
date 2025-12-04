// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Admin.Core.Services.UserNotifications;

public class GroupedUserNotificationRenderer
{
    private readonly UrlConfig _urlConfig;

    public GroupedUserNotificationRenderer(UrlConfig urlConfig)
    {
        _urlConfig = urlConfig;
    }

    public UserNotification Render(string recipientEmail, List<UserNotificationEntity> notifications)
    {
        var groups = BuildGroups(notifications);
        return new UserNotification(recipientEmail, RenderSubject(groups).Truncate(100), RenderHtml(groups));
    }

    private static string Html([StringSyntax("html")] string html) => html;

    private string RenderSubject(IReadOnlyList<CollectionGroup> groups)
        => $"E-Collecting: Änderungen in {string.Join(", ", groups.Select(x => x.CollectionName))}";

    private string RenderHtml(IReadOnlyList<CollectionGroup> groups)
    {
        var renderedGroups = string.Join(Environment.NewLine, groups.Select(RenderEntryHtml));

        return Html($$"""
                 <!DOCTYPE html>
                 <html lang="de-CH">
                 <head>
                   <meta charset="UTF-8" />
                   <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
                   <title>Neuigkeiten in E-Collecting</title>
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
                   </style>
                 </head>
                 <body>
                   <div class="container">
                     <h2>Neuigkeiten in E-Collecting</h2>
                     <p>Hallo,</p>
                     <p>im E-Collecting gibt es Neuigkeiten:</p>
                     <ul>
                        {{renderedGroups}}
                     </ul>
                   </div>
                 </body>
                 </html>
                 """);
    }

    private string RenderEntryHtml(CollectionGroup group)
    {
        var types = string.Join(Environment.NewLine + "  ", group.Types.Select(TypeTextHtml));

        return Html(
            $"""
             <li>
                <a href="{UserNotificationRenderer.EncodeHref(group.CollectionUrl)}">{UserNotificationRenderer.EncodeHtml(group.CollectionName)}</a><br />
                {string.Join("<br />", types)}
             </li>
             """);
    }

    private string TypeTextHtml(UserNotificationType type)
    {
        var text = type switch
        {
            UserNotificationType.MessageAdded => "Es ist eine neue Nachricht verfügbar.",
            UserNotificationType.StateChanged => "Der Status hat sich geändert.",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        return UserNotificationRenderer.EncodeHtml(text);
    }

    private IReadOnlyList<CollectionGroup> BuildGroups(List<UserNotificationEntity> notifications)
    {
        return notifications
            .Where(x => x.TemplateBag is { CollectionId: not null, CollectionType: not null })
            .GroupBy(x => x.TemplateBag.CollectionId!.Value)
            .Select(g => new CollectionGroup(
                g.First().TemplateBag.CollectionName,
                _urlConfig.BuildCollectionUrl(g.Key, g.First().TemplateBag.CollectionType!.Value, g.First().TemplateBag.RecipientIsCitizen),
                g.Select(x => x.TemplateBag.NotificationType).Distinct().ToList()))
            .ToList();
    }

    private sealed record CollectionGroup(
        string CollectionName,
        string CollectionUrl,
        IReadOnlyList<UserNotificationType> Types);
}
