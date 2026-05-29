ARG DOTNET_SDK_VERSION=10.0
ARG DOTNET_RUNTIME_VERSION=10.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS build
WORKDIR /src

COPY ["WebMonitorBot.csproj", "./"]
RUN dotnet restore "WebMonitorBot.csproj"

COPY . ./
RUN dotnet publish "WebMonitorBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_RUNTIME_VERSION} AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "WebMonitorBot.dll"]
