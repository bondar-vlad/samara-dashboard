using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Infrastructure.Hosting;
using ChildRights.BuildingBlocks.Infrastructure.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Persistence;
using ChildRights.Social.Api.Messaging;
using ChildRights.Social.Api.Persistence;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("Social Services API");
builder.Services.AddApiControllers();
builder.Services.AddPostgresDbContext<SocialDbContext>(builder.Configuration);
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SocialDbContext>());
builder.Services.AddEventBus(builder.Configuration, cfg => cfg.AddConsumer<ReferralReceivedConsumer>());

var app = builder.Build();

app.UseServiceDefaults();
app.MapControllers();

await app.InitializeDatabaseAsync<SocialDbContext>();

await app.RunAsync();
