# syntax=docker/dockerfile:1

# Multi-stage build shared by every microservice. The specific project is selected
# via build args, so all services use this one Dockerfile.
#   PROJECT_PATH — path to the .csproj (relative to repo root)
#   PROJECT_DLL  — the produced entry assembly name

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG PROJECT_PATH
WORKDIR /src

# Copy solution-level files first so restore is cached independently of source changes.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/ ./src/

RUN dotnet restore "${PROJECT_PATH}"
RUN dotnet publish "${PROJECT_PATH}" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
ARG PROJECT_DLL
ENV PROJECT_DLL=${PROJECT_DLL}
ENV ASPNETCORE_HTTP_PORTS=8080
WORKDIR /app
COPY --from=build /app/publish .

# PROJECT_DLL is fixed at build time; expand it into the entrypoint here.
RUN printf '#!/bin/sh\nexec dotnet "%s"\n' "$PROJECT_DLL" > /app/entrypoint.sh \
    && chmod +x /app/entrypoint.sh

EXPOSE 8080
ENTRYPOINT ["/app/entrypoint.sh"]
