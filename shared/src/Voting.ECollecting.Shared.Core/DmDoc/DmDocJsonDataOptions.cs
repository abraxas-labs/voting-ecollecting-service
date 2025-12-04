// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Voting.ECollecting.Shared.Core.DmDoc;

internal static class DmDocJsonDataOptions
{
    internal static readonly JsonSerializerOptions Instance = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };
}
