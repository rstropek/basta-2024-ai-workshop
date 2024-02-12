using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConferenceProgram;

/// <summary>
/// Interface for reading conference program sessions.
/// </summary>
/// <seealso cref="ConferenceProgramReader"/>
/// <remarks>
/// This interface can be used for testing purposes (mocking, etc.).
/// </remarks>
public interface IConferenceProgramReader
{
    Task<IReadOnlyList<Session>> ReadSessionsAsync(string? fileName = null);
}

/// <summary>
/// Reads conference program sessions from a file.
/// </summary>
public class ConferenceProgramReader : IConferenceProgramReader
{
    /// <summary>
    /// Reads the sessions from the specified file.
    /// </summary>
    /// <param name="fileName">Name of the file to read</param>
    /// <returns>
    /// Sessions read from the file.
    /// </returns>
    public async Task<IReadOnlyList<Session>> ReadSessionsAsync(string? fileName = null)
    {
        fileName ??= Path.Combine(
            Path.GetDirectoryName(this.GetType().Assembly.Location) ?? string.Empty, 
            "sessions.json");
        
        using FileStream fileStream = new(fileName, FileMode.Open);
        var sessions = await JsonSerializer.DeserializeAsync(fileStream, SessionsSerializerContext.Default.IReadOnlyListSession);
        Debug.Assert(sessions is not null);
        return sessions;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(IReadOnlyList<Session>))]
public partial class SessionsSerializerContext : JsonSerializerContext { }
