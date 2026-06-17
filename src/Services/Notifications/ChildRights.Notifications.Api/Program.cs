using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Infrastructure.Hosting;
using ChildRights.BuildingBlocks.Infrastructure.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Persistence;
using ChildRights.Notifications.Api.Messaging;
using ChildRights.Notifications.Api.Persistence;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("Notifications API");
builder.Services.AddApiControllers();
builder.Services.AddPostgresDbContext<NotificationsDbContext>(builder.Configuration);
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NotificationsDbContext>());
builder.Services.AddEventBus(builder.Configuration, cfg => cfg.AddConsumer<RedFlagRaisedConsumer>());

var app = builder.Build();

app.UseServiceDefaults();
app.MapControllers();

await app.InitializeDatabaseAsync<NotificationsDbContext>();

await app.RunAsync();
