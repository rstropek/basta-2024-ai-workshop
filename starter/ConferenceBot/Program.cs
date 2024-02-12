var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapGet("/ping", () => "pong");

app.Run();
