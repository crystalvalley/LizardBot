var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "LizardControlBot is running.");
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "LizardControlBot"
}));

app.Run();