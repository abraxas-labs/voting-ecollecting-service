// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class FileContentEntity : BaseEntity
{
    public byte[] Data { get; set; } = [];

    public Guid FileId { get; set; }

    public FileEntity? File { get; set; }
}
