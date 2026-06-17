<#
.SYNOPSIS
    Runs all six microservices and the API gateway locally (without Docker).
.DESCRIPTION
    Each service is launched in its own window. Requires a local PostgreSQL on
    localhost:5432 (postgres/postgres). RabbitMQ is optional — without it the
    services fall back to an in-memory bus (single-process eventing only), so for
    full cross-service messaging start RabbitMQ or use docker-compose instead.
#>

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

$services = @(
    "src/Services/Education/ChildRights.Education.Api",
    "src/Services/Social/ChildRights.Social.Api",
    "src/Services/Medical/ChildRights.Medical.Api",
    "src/Services/JuvenilePolice/ChildRights.JuvenilePolice.Api",
    "src/Services/Analysis/ChildRights.Analysis.Api",
    "src/Services/Notifications/ChildRights.Notifications.Api",
    "src/ApiGateway/ChildRights.ApiGateway"
)

foreach ($service in $services) {
    $path = Join-Path $root $service
    Write-Host "Starting $service ..." -ForegroundColor Cyan
    Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$path`"" -WorkingDirectory $root
    Start-Sleep -Seconds 2
}

Write-Host ""
Write-Host "All services starting. Swagger UIs:" -ForegroundColor Green
Write-Host "  Education      http://localhost:5101/swagger"
Write-Host "  Social         http://localhost:5102/swagger"
Write-Host "  Medical        http://localhost:5103/swagger"
Write-Host "  JuvenilePolice http://localhost:5104/swagger"
Write-Host "  Analysis       http://localhost:5105/swagger"
Write-Host "  Notifications  http://localhost:5106/swagger"
Write-Host "  Gateway        http://localhost:8080"
