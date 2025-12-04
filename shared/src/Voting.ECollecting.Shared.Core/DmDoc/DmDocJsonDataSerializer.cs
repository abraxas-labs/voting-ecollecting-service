// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text.Json;
using Voting.Lib.DmDoc.Serialization;

namespace Voting.ECollecting.Shared.Core.DmDoc;

public class DmDocJsonDataSerializer : IDmDocDataSerializer
{
    public string MimeType => "application/json";

    public string Serialize<T>(T data)
    {
        return JsonSerializer.Serialize(data, DmDocJsonDataOptions.Instance);
    }
}
