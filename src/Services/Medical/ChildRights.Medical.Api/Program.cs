using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Infrastructure.Hosting;
using ChildRights.BuildingBlocks.Infrastructure.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Persistence;
using ChildRights.Medical.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("Medical API");
builder.Services.AddApiControllers();
builder.Services.AddPostgresDbContext<MedicalDbContext>(builder.Configuration);
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MedicalDbContext>());
builder.Services.AddScoped<IDataSeeder, MedicalDataSeeder>();
builder.Services.AddEventBus(builder.Configuration);

var app = builder.Build();

app.UseServiceDefaults();
app.MapControllers();

await app.InitializeDatabaseAsync<MedicalDbContext>();

await app.RunAsync();
