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
# Expect the following environment variables to be provided at runtime:
# - TELEGRAM_BOT_TOKEN : token del bot de Telegram (required)
# - DATABASE_CONN : cadena de conexión a la base de datos (required)
# - DEEPSEEK_API_KEY : clave de API para Deepseek (required)

# Add placeholder env vars so 'docker inspect' shows them and they are easier to override
ENV TELEGRAM_BOT_TOKEN=""
ENV DATABASE_CONN=""
ENV DEEPSEEK_API_KEY=""

ENTRYPOINT ["dotnet", "WebMonitorBot.dll"]
