﻿using System.Text.Json.Nodes;
using Domain.Contracts;
using JetBrains.Annotations;

namespace Domain.Tools;

[UsedImplicitly]
public record FileMoveParams
{
    public required string SourcePath { get; [UsedImplicitly] init; }
    public required string DestinationPath { get; [UsedImplicitly] init; }
}

public class MoveTool(
    IFileSystemClient client,
    string libraryPath) : BaseTool<MoveTool, FileMoveParams>, IToolWithMetadata
{
    public static string Name => "Move";

    public static string Description => """
                                        Moves and/or renames a file or directory. Both arguments have to be absolute 
                                        paths and must be derived from the LibraryDescription tool response.
                                        Equivalent to 'mv -T {SourcePath} {DestinationPath}' bash command.
                                        The destination path MUST NOT exist, otherwise an exception will be thrown.
                                        All necessary parent directories will be created automatically.
                                        """;

    public async Task<JsonNode> Run(JsonNode? parameters, CancellationToken cancellationToken = default)
    {
        var typedParams = ParseParams(parameters);

        if (!typedParams.SourcePath.StartsWith(libraryPath) ||
            !typedParams.DestinationPath.StartsWith(libraryPath))
        {
            throw new ArgumentException($"""
                                         {typeof(MoveTool)} parameters must be absolute paths derived from the 
                                         LibraryDescription tool response. 
                                         They must start with the library path: {libraryPath}
                                         """);
        }

        await client.Move(typedParams.SourcePath, typedParams.DestinationPath, cancellationToken);
        return new JsonObject
        {
            ["status"] = "success",
            ["message"] = "File moved successfully",
            ["source"] = typedParams.SourcePath,
            ["destination"] = typedParams.DestinationPath
        };
    }
}