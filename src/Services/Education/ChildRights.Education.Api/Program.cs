using ChildRights.BuildingBlocks.Infrastructure.Hosting;
using ChildRights.BuildingBlocks.Infrastructure.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Persistence;
using ChildRights.Education.Application;
using ChildRights.Education.Infrastructure;
using ChildRights.Education.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("Education API");
builder.Services.AddApiControllers();
builder.Services.AddEducationApplication();
builder.Services.AddEducationInfrastructure(builder.Configuration);
builder.Services.AddEventBus(builder.Configuration);

var app = builder.Build();

app.UseServiceDefaults();
app.MapControllers();

await app.InitializeDatabaseAsync<EducationDbContext>();

await app.RunAsync();
