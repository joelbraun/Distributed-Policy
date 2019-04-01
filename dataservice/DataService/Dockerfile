FROM mcr.microsoft.com/dotnet/core/runtime:2.1-stretch-slim AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["DataService/DataService.csproj", "DataService/"]
RUN dotnet restore "DataService/DataService.csproj"
COPY . .
WORKDIR "/src/DataService"
RUN dotnet build "DataService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DataService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

RUN GRPC_HEALTH_PROBE_VERSION=v0.2.0 && \
    wget -qO/bin/grpc_health_probe https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/${GRPC_HEALTH_PROBE_VERSION}/grpc_health_probe-linux-amd64 && \
    chmod +x /bin/grpc_health_probe

ENTRYPOINT ["dotnet", "DataService.dll"]