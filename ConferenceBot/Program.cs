using ConferenceBot;
using ConferenceProgram;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddCors();
builder.Services.AddRateLimiting(builder.Configuration.GetSection("RateLimitOptions"));
builder.Services.AddOpenAIClient(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddTransient<IStreamProcessor, StreamProcessor>();
builder.Services.AddSingleton<IChatManager, ChatManager>();
builder.Services.AddSingleton<IAiFunctions, AiFunctions>();
builder.Services.ConfigureJsonOptions([
    ChatCompletionsApiSerializerContext.Default, 
    SessionsSerializerContext.Default,
    AiFunctionsSerializerContext.Default
]);

var reader = new ConferenceProgramReader();
var sessions = await reader.ReadSessionsAsync(/* use default file sessions.json */);
builder.Services.AddSingleton<IEnumerable<Session>>(sessions);

var app = builder.Build();

app.UseDefaultExceptionHandling();
app.UseRouting();
app.UseRateLimiter();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapGet("/ping", () => "pong").RequireRateLimiting(RateLimitOptionsExtensions.PolicyName);
app.MapChatCompletions();

app.Run();
