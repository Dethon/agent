﻿using System.Text.Json.Nodes;
using Domain.Contracts;
using Domain.DTOs;
using JetBrains.Annotations;

namespace Domain.Tools;

[UsedImplicitly]
public record CleanupParams
{
    public required int DownloadId { get; [UsedImplicitly] init; }
}

public class CleanupTool(
    IDownloadClient downloadClient,
    IFileSystemClient fileSystemClient,
    string baseDownloadLocation) : BaseTool, ITool
{
    public string Name => "Cleanup";

    public async Task<JsonNode> Run(JsonNode? parameters, CancellationToken cancellationToken = default)
    {
        var typedParams = ParseParams<CleanupParams>(parameters);
        var downloadPath = $"{baseDownloadLocation}/{typedParams.DownloadId}";

        await fileSystemClient.RemoveDirectory(downloadPath, cancellationToken);
        await downloadClient.Cleanup(typedParams.DownloadId, cancellationToken);

        return new JsonObject
        {
            ["status"] = "success",
            ["message"] = "Download leftovers removed successfully",
            ["downloadId"] = typedParams.DownloadId
        };
    }

    public ToolDefinition GetToolDefinition()
    {
        return new ToolDefinition<CleanupParams>
        {
            Name = Name,
            Description = """
                          Removes a everything that is left over in a download directory.
                          It can also be use to cancel a download if the user requests it.
                          """
        };
    }
}