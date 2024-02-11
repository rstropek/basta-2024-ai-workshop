using System.Collections.Concurrent;
using System.Diagnostics;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;

namespace ConferenceBot;

public interface IChatManager
{
    IChatSession CreateSession();
    IChatSession? GetSession(Guid sessionId);
}

public class ChatManager(OpenAIClient client, IOptions<OpenAISettings> settings, ILogger<ChatManager> logger) : IChatManager
{
    private readonly ConcurrentDictionary<Guid, ChatSession> Sessions = [];

    public IChatSession CreateSession()
    {
        var session = new ChatSession(client, settings, logger);
        var added = Sessions.TryAdd(session.Id, session);
        Debug.Assert(added, "Created a new guid, add should always succeed");
        return session;
    }

    public IChatSession? GetSession(Guid sessionId)
    {
        if (Sessions.TryGetValue(sessionId, out var session))
        {
            return session;
        }

        return null;
    }
}
