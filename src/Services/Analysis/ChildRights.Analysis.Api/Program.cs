using ChildRights.Analysis.Application;
using ChildRights.Analysis.Infrastructure;
using ChildRights.Analysis.Infrastructure.Persistence;
using ChildRights.BuildingBlocks.Infrastructure.Hosting;
using ChildRights.BuildingBlocks.Infrastructure.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("Analysis API");
builder.Services.AddApiControllers();
builder.Services.AddAnalysisApplication();
builder.Services.AddAnalysisInfrastructure(builder.Configuration);
builder.Services.AddEventBus(builder.Configuration, ChildRights.Analysis.Infrastructure.DependencyInjection.AddAnalysisConsumers);

var app = builder.Build();

app.UseServiceDefaults();
app.MapControllers();

await app.InitializeDatabaseAsync<AnalysisDbContext>();

await app.RunAsync();
