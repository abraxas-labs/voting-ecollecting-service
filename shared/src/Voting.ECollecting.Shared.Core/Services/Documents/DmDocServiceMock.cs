// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Runtime.CompilerServices;
using Voting.Lib.DmDoc;
using Voting.Lib.DmDoc.Models;

namespace Voting.ECollecting.Shared.Core.Services.Documents;

public class DmDocServiceMock : IDmDocService
{
    public Task<List<Category>> ListCategories(CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Template> GetTemplate(int id, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<List<Template>> ListTemplates(CancellationToken ct = default) => throw new NotImplementedException();

    public Task<List<Template>> ListTemplates(string category, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<List<Category>> ListTemplateCategories(int templateId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<List<DataContainer>> ListTemplateDataContainers(
        int templateId,
        bool includeSystemContainer = false,
        bool includeUserContainer = true,
        CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Draft> CreateDraft<T>(
        int templateId,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Draft> CreateDraft<T>(
        string templateName,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Draft> GetDraft(int draftId, CancellationToken ct = default) => throw new NotImplementedException();

    public Task DeleteDraft(int draftId, CancellationToken ct = default) => throw new NotImplementedException();

    public Task DeleteDraftContent(int draftId, CancellationToken ct = default) => throw new NotImplementedException();

    public Task DeleteDraftHard(int draftId, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<Stream> PreviewDraftAsPdf(int draftId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Stream> PreviewAsPdf<T>(
        int templateId,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Stream> PreviewAsPdf<T>(
        string templateName,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Stream> FinishDraftAsPdf(int draftId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Stream>
        FinishAsPdf<T>(int templateId, T templateData, string? bulkRoot, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public async Task<Stream> FinishAsPdf<T>(
        string templateName,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), ct);
        return File.OpenRead(GetMockPdfFilePath());
    }

    public Task<Draft> StartAsyncPdfGeneration<T>(
        int templateId,
        T templateData,
        string webhookEndpoint,
        string? bulkRoot,
        CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Draft> StartAsyncPdfGeneration<T>(
        string templateName,
        T templateData,
        string webhookEndpoint,
        string? bulkRoot = null,
        CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<Stream> GetPdfForPrintJob(
        int printJobId,
        CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<List<Brick>> ListBricks(CancellationToken ct = default) => throw new NotImplementedException();

    public Task<List<Brick>> ListBricks(int categoryId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<List<Brick>> ListBricks(string category, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<List<Brick>> ListActiveBricks(string category, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<string> GetBrickContentEditorUrl(int brickId, int brickContentId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<(int NewBrickId, int NewContentId)> UpdateBrickContent(
        int brickContentId,
        string content,
        CancellationToken ct = default) => throw new NotImplementedException();

    public Task TagBricks(int[] brickIds, string tag, CancellationToken ct = default) =>
        throw new NotImplementedException();

    private string GetMockPdfFilePath([CallerFilePath] string path = "")
    {
        return Path.Join(
            Path.GetDirectoryName(path),
            "..",
            "..",
            "..",
            "..",
            "..",
            "tools",
            "Voting.ECollecting.DataSeeder.Data",
            "DataSets",
            "Files",
            "placeholder-signatures.pdf");
    }
}
