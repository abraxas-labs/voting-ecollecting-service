// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Buffers.Binary;
using Voting.Lib.Cryptography.Exceptions;

namespace Voting.ECollecting.Shared.Core.Services;

/// <summary>
/// Guid serializer according to https://confluence.abraxas-tools.ch/confluence/spaces/ECOLP/pages/402893429/Duplicate+Detection+of+VOTING+Stimmregister+ID
/// (always use big endian).
/// </summary>
public static class CollectionCryptoIdSerializer
{
    private const int GuidByteLength = 16;

    public static byte[] SerializeStimmregisterId(Guid registerId)
    {
        Span<byte> idBytes = stackalloc byte[GuidByteLength];
        if (!registerId.TryWriteBytes(idBytes, true, out _))
        {
            throw new CryptographyException("Could not serialize stimmregister id");
        }

        // convert to array as the HSM api doesn't provide a span interface.
        return idBytes.ToArray();
    }

    public static Guid DeserializeStimmregisterId(ReadOnlySpan<byte> data)
    {
        var a = BinaryPrimitives.ReadInt32BigEndian(data[..4]);
        var b = BinaryPrimitives.ReadInt16BigEndian(data.Slice(4, 2));
        var c = BinaryPrimitives.ReadInt16BigEndian(data.Slice(6, 2));

        return new Guid(
            a,
            b,
            c,
            data[8],
            data[9],
            data[10],
            data[11],
            data[12],
            data[13],
            data[14],
            data[15]);
    }
}
