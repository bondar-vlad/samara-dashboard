using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Infrastructure.Hosting;
using ChildRights.BuildingBlocks.Infrastructure.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Persistence;
using ChildRights.JuvenilePolice.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("Juvenile Police API");
builder.Services.AddApiControllers();
builder.Services.AddPostgresDbContext<JuvenilePoliceDbContext>(builder.Configuration);
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<JuvenilePoliceDbContext>());
builder.Services.AddEventBus(builder.Configuration);

var app = builder.Build();

app.UseServiceDefaults();
app.MapControllers();

await app.InitializeDatabaseAsync<JuvenilePoliceDbContext>();

await app.RunAsync();
