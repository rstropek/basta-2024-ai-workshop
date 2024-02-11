using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using ConferenceProgram;

namespace ConferenceBot;

public interface IAiFunctions
{
    void AddFunctionsToChatCompletionsOptions(ChatCompletionsOptions options);
    ChatRequestToolMessage Execute(FunctionCall call);
}

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
            // Currently, JSON schema must be specified as a string as reflection-based
            // serialization is not supported with NativeAOT. See also
            // https://github.com/Azure/azure-sdk-for-net/issues/41893
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

    public void AddFunctionsToChatCompletionsOptions(ChatCompletionsOptions options)
    {
        if (options.Tools.Count == 0)
        {
            options.Tools.Add(GetExpertsTool);
            options.Tools.Add(GetSessionsByExpert);
        }
    }

    public ChatRequestToolMessage Execute(FunctionCall call)
    {
        ChatRequestToolMessage result;
        switch (call.Name)
        {
            case "getExperts":
                {
                    var experts = sessions.GetExperts().ToArray();
                    result = new ChatRequestToolMessage(JsonSerializer.Serialize(experts, AiFunctionsSerializerContext.Default.ExpertSummaryArray)!, call.Id);
                    break;
                }
            case "getSessionsByExpert":
                {
                    var args = JsonSerializer.Deserialize(call.ArgumentJson.ToString(), AiFunctionsSerializerContext.Default.SessionsByExpertArguments);
                    if (args == null || string.IsNullOrEmpty(args.Forename) || string.IsNullOrEmpty(args.Surname))
                    {
                        result = new ChatRequestToolMessage(JsonSerializer.Serialize(new ErrorResponse("Invalid arguments"), AiFunctionsSerializerContext.Default.ErrorResponse)!, call.Id);
                        break;
                    }

                    var sessionsByExpert = sessions.GetSessionsByExpert(args.Forename, args.Surname).ToArray();
                    result = new ChatRequestToolMessage(JsonSerializer.Serialize(sessionsByExpert, AiFunctionsSerializerContext.Default.SessionSummaryArray)!, call.Id);
                    break;
                }
            default:
                {
                    result = new ChatRequestToolMessage(JsonSerializer.Serialize(new ErrorResponse("Unknown function name"), AiFunctionsSerializerContext.Default.ErrorResponse)!, call.Id);
                    break;
                }
        }

        return result;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ExpertSummary[]))]
[JsonSerializable(typeof(SessionSummary[]))]
[JsonSerializable(typeof(SessionsByExpertArguments))]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class AiFunctionsSerializerContext : JsonSerializerContext { }

record SessionsByExpertArguments(string Forename, string Surname);

record ErrorResponse(string Error);
