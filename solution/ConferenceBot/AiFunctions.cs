using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using ConferenceProgram;

namespace ConferenceBot;

/// <summary>
/// Abstraction for <see cref="AiFunctions"/> to make it easier to test.
/// </summary>
public interface IAiFunctions
{
    void EnsureFunctionsAreInCompletionsOptions(ChatCompletionsOptions options);
    ChatRequestToolMessage Execute(FunctionCall call);
}

/// <summary>
/// Implements function tools for our example.
/// </summary>
/// <param name="sessions">Collection of conference samples</param>
public class AiFunctions(IEnumerable<Session> sessions) : IAiFunctions
{
    private readonly ChatCompletionsFunctionToolDefinition GetExpertsTool = new(
        new FunctionDefinition()
        {
            Name = "getExperts",
            Description = """
                Returns a list of experts speaking at the conference. Experts are people
                who are speaking at one or more sessions. The function also returns
                the company where the export works.
                """,
            // Parameters must be in JSON Schema format. You could create the JSON using
            // System.Text.Json, but specifying the JSON as a string is also an option
            // (and sometimes easier). It would be nice to derive the schema from code
            // (e.g. using a source generator), but that is not supported yet.
            Parameters = BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": { }
                }
                """)
        });

    private readonly ChatCompletionsFunctionToolDefinition GetSessionsByExpert = new(
        new FunctionDefinition()
        {
            Name = "getSessionsByExpert",
            Description = """
                Returns a list of sessions that a given expert is speaking at.
                Each record contains the session name, and the start and end time of the session.
                The list of experts can be obtained using the `getExperts` function.
                """,
            Parameters = BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "forename": {
                            "type": "string",
                            "description": "The forename (aka firstname) of the expert"
                        },
                        "surname": {
                            "type": "string",
                            "description": "The surname (aka lastname) of the expert"
                        }
                    },
                    "required": ["forename", "surname"]
                }
                """)
        });

    /// <summary>
    /// Ensures that the function tools are present in the given options.
    /// </summary>
    /// <param name="options">Options to which the function tools should be added</param>
    public void EnsureFunctionsAreInCompletionsOptions(ChatCompletionsOptions options)
    {
        if (options.Tools.Count == 0)
        {
            options.Tools.Add(GetExpertsTool);
            options.Tools.Add(GetSessionsByExpert);
        }
    }

    /// <summary>
    /// Execute a given function call
    /// </summary>
    /// <param name="call"></param>
    /// <returns>
    /// Result of the function call
    /// </returns>
    public ChatRequestToolMessage Execute(FunctionCall call)
    {
        // Note that we do no throw an exception here if something goes wrong. Instead, we
        // let ChatGPT know, that something is wrong so that it can fix its own mistake.

        string resultJson;
        switch (call.Name)
        {
            case "getExperts":
                {
                    var experts = sessions.GetExperts().ToArray();
                    resultJson = JsonSerializer.Serialize(experts, AiFunctionsSerializerContext.Default.ExpertSummaryArray);
                    break;
                }
            case "getSessionsByExpert":
                {
                    var args = JsonSerializer.Deserialize(call.ArgumentJson.ToString(), AiFunctionsSerializerContext.Default.SessionsByExpertArguments);
                    if (args == null || string.IsNullOrEmpty(args.Forename) || string.IsNullOrEmpty(args.Surname))
                    {
                        resultJson = JsonSerializer.Serialize(new ErrorResponse("Invalid arguments"), AiFunctionsSerializerContext.Default.ErrorResponse);
                        break;
                    }

                    var sessionsByExpert = sessions.GetSessionsByExpert(args.Forename, args.Surname).ToArray();
                    resultJson = JsonSerializer.Serialize(sessionsByExpert, AiFunctionsSerializerContext.Default.SessionSummaryArray);
                    break;
                }
            default:
                {
                    resultJson = JsonSerializer.Serialize(new ErrorResponse("Unknown function name"), AiFunctionsSerializerContext.Default.ErrorResponse);
                    break;
                }
        }

        return new ChatRequestToolMessage(resultJson!, call.Id);
    }

    internal record ErrorResponse(string Error);

    internal record SessionsByExpertArguments(string Forename, string Surname);
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ExpertSummary[]))]
[JsonSerializable(typeof(SessionSummary[]))]
[JsonSerializable(typeof(AiFunctions.SessionsByExpertArguments))]
[JsonSerializable(typeof(AiFunctions.ErrorResponse))]
internal partial class AiFunctionsSerializerContext : JsonSerializerContext { }
