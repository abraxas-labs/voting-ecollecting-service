// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using HeyRed.Mime;
using Microsoft.IO;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.MalwareScanner.Services;

namespace Voting.ECollecting.Shared.Core.Services;

public class FileService : IFileService
{
    private const char LeadingFileExtensionChar = '.';
    private const char ContentTypeCharsetSeparator = ';';
    private readonly IMalwareScannerService _malwareScannerService;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    public FileService(RecyclableMemoryStreamManager memoryStreamManager, IMalwareScannerService malwareScannerService)
    {
        _memoryStreamManager = memoryStreamManager;
        _malwareScannerService = malwareScannerService;
    }

    public async Task<FileEntity> Validate(
        Stream file,
        [NotNull] string? contentType,
        [NotNull] string? fileName,
        IReadOnlySet<string> allowedFileExtensions,
        bool validateMimeType = true,
        CancellationToken ct = default)
    {
        // Usually http streams don't supports seeking, so we copy the content (is never more than 10MB anyway)
        await using var fileContentStream = _memoryStreamManager.GetStream();

        await file.CopyToAsync(fileContentStream, ct);
        fileContentStream.Seek(0, SeekOrigin.Begin);

        await EnsureValidFileContent(fileContentStream, contentType, fileName, allowedFileExtensions, validateMimeType, ct);
        fileContentStream.Seek(0, SeekOrigin.Begin);

        return new FileEntity
        {
            Content = new FileContentEntity { Data = fileContentStream.ToArray() },
            ContentType = contentType,
            Name = fileName,
        };
    }

    private async Task EnsureValidFileContent(
        Stream contentStream,
        [NotNull] string? mimeType,
        [NotNull] string? fileName,
        IReadOnlySet<string> allowedExtensions,
        bool validateMimeType = true,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(mimeType) || string.IsNullOrEmpty(fileName))
        {
            throw new ValidationException("Both the MIME type (content type) and the file name must be provided");
        }

        var extensionFromFileName = Path.GetExtension(fileName)
            .TrimStart(LeadingFileExtensionChar)
            .ToLowerInvariant();

        // jpg is the same as jpeg, content type from a .jpg image and guessing mime type will always result in jpeg
        if (extensionFromFileName == "jpg")
        {
            extensionFromFileName = "jpeg";
        }

        if (validateMimeType)
        {
            ValidateMimeType(contentStream, mimeType, extensionFromFileName);
        }

        if (!allowedExtensions.Contains(extensionFromFileName))
        {
            throw new ValidationException($"File extension {extensionFromFileName} is not allowed for uploads");
        }

        await _malwareScannerService.EnsureFileIsClean(contentStream, ct);
    }

    private void ValidateMimeType(Stream contentStream, string mimeType, string extensionFromFileName)
    {
        var extensionFromReceivedMimeType = MimeTypesMap.GetExtension(mimeType.Split(ContentTypeCharsetSeparator)[0]);
        contentStream.Seek(0, SeekOrigin.Begin);
        var guessedMimeType = MimeGuesser.GuessMimeType(contentStream);
        var extensionFromGuessedMimeType = MimeTypesMap.GetExtension(guessedMimeType);

        if (extensionFromFileName != extensionFromReceivedMimeType || extensionFromFileName != extensionFromGuessedMimeType)
        {
            throw new ValidationException(
                $"File extensions differ. From file name: {extensionFromFileName}, from content type: {extensionFromReceivedMimeType}, guessed from content: {extensionFromGuessedMimeType}");
        }
    }
}
