﻿using Domain.Contracts;
using Domain.Tools;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Domain.Agents;

public class AgentResolver(
    DownloaderPrompt downloaderPrompt,
    ILargeLanguageModel languageModel,
    FileDownloadTool fileDownloadTool,
    FileSearchTool fileSearchTool,
    WaitForDownloadTool waitForDownloadTool,
    MoveTool moveTool,
    CleanupTool cleanupTool,
    ListDirectoriesTool listDirectoriesTool,
    ListFilesTool listFilesTool,
    IMemoryCache cache,
    ILoggerFactory loggerFactory) : IAgentResolver
{
    public async Task<IAgent> Resolve(AgentType agentType, int? sourceMessageId = null)
    {
        return GetAgentFromCache(sourceMessageId) ?? agentType switch
        {
            AgentType.Download => new Agent(
                messages: await downloaderPrompt.Get(null),
                largeLanguageModel: languageModel,
                tools:
                [
                    fileSearchTool,
                    fileDownloadTool,
                    waitForDownloadTool,
                    listDirectoriesTool,
                    listFilesTool,
                    moveTool,
                    cleanupTool
                ],
                maxDepth: 10,
                enableSearch: false,
                logger: loggerFactory.CreateLogger<Agent>()),
            _ => throw new ArgumentException($"Unknown agent type: {agentType}")
        };
    }

    public void AssociateMessageToAgent(int messageId, IAgent agent)
    {
        cache.Set($"IAgent{messageId}", agent, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddMonths(2)
        });
    }

    private IAgent? GetAgentFromCache(int? sourceMessageId)
    {
        if (sourceMessageId.HasValue && cache.TryGetValue($"IAgent{sourceMessageId}", out IAgent? agent))
        {
            return agent;
        }

        return null;
    }
}