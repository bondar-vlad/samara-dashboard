using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((_, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");
app.MapReverseProxy();

// Simple landing page that documents how to reach each service through the gateway.
app.MapGet("/", () => Results.Json(new
{
    service = "Child Rights Monitoring — API Gateway",
    health = "/health",
    apiRoutes = new
    {
        education = "/education/api/students",
        social = "/social/api/social/cases",
        medical = "/medical/api/medical/visits",
        juvenilePolice = "/juvenile/api/juvenile/bullying-reports",
        analysis = "/analysis/api/dashboard/summary",
        notifications = "/notifications/api/notifications"
    },
    note = "Interactive Swagger UI is served on each service's own port — see the README."
}));

app.Run();
