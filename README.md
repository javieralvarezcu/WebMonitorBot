# WebMonitorBot

Descripción
-----------
WebMonitorBot es un servicio .NET (target: .NET 10) que monitoriza páginas web y notifica cambios a través de un bot de Telegram. Extrae el texto visible de las páginas, calcula diferencias y opcionalmente usa un cliente LLM (Deepseek por defecto) para generar un resumen semántico de los cambios.

Principales funcionalidades
--------------------------
- Registrar URLs desde Telegram y asociarlas a un chat.
- Comprobación periódica de páginas web con intervalo configurable por URL.
- Extracción de texto limpio (sin scripts, estilos, nav, footer) mediante AngleSharp.
- Detección de cambios por hash y resumen de diferencias mediante un LLM (Deepseek) o una implementación simulada.
- Control de acceso mediante una whitelist de chats almacenada en base de datos.
- Persistencia de configuración y estado en SQL Server usando Entity Framework Core.

Estructura del proyecto
-----------------------
- WebMonitorBot/
  - Bot/: servicio hospedado que maneja interacciones con Telegram.
  - Data/: contratos, modelos y repositorios (EF Core).
  - Services/: extracción HTML, monitor, cliente LLM y adaptadores.
  - Program.cs: arranque, DI, configuración y migraciones automáticas.
  - appsettings.json: configuración local de ejemplo.

Requisitos
----------
- .NET 10 SDK
- SQL Server accesible (puede ser local o remoto)
- Cuenta / token de Telegram para el bot
- (Opcional) API key de Deepseek para análisis semántico

Configuración
-------------
Los valores principales se configuran en appsettings.json o mediante variables de entorno. Claves relevantes:

- Telegram:BotToken
  - Token del bot de Telegram. Alternativa: variable de entorno TELEGRAM_BOT_TOKEN.
- Deepseek:ApiKey
  - API key para el cliente Deepseek. Alternativa: variable de entorno DEEPSEEK_API_KEY.
- ConnectionStrings:Default
  - Cadena de conexión a SQL Server. Alternativa: variable de entorno WEBMONITORBOT_CONN.
- Monitoring:DefaultIntervalSeconds
  - Intervalo por defecto en segundos si una URL no especifica intervalo propio.

Ejemplo (appsettings.json)
--------------------------
{ "Telegram": { "BotToken": "<token>" }, "Deepseek": { "ApiUrl": "https://api.deepseek.com/v1/chat" }, "ConnectionStrings": { "Default": "<connection-string>" }, "Monitoring": { "DefaultIntervalSeconds": 60 } }

Variables de entorno
--------------------
- TELEGRAM_BOT_TOKEN: token del bot (si no está en appsettings.json)
- DEEPSEEK_API_KEY: API key de Deepseek (requerida si se usa DeepseekLlmClient)
- WEBMONITORBOT_CONN: cadena de conexión alternativa

Cómo ejecutar en desarrollo
---------------------------
1. Clonar el repositorio.
2. Abrir la solución en Visual Studio 2026 o usar la CLI de dotnet.
3. Configurar appsettings.json o exportar las variables de entorno necesarias.
4. Desde Visual Studio: ejecutar la aplicación. Desde CLI:

   dotnet run --project WebMonitorBot

Notas importantes al arranque
-----------------------------
- Program.cs aplica automáticamente las migraciones de EF Core al iniciar la aplicación. Asegúrate de que la cadena de conexión esté correcta y que el usuario de la base de datos tenga permisos para crear/alterar tablas.
- Si el token de Telegram o la API key de Deepseek no están configurados y son requeridos, la aplicación lanzará una excepción al arrancar.

Uso del bot de Telegram (comandos)
---------------------------------
- /monitor <url> [seconds] — Registrar una URL para monitoreo. El parámetro opcional [seconds] define el intervalo de comprobación para esa URL.
- /setinterval <url> <seconds> — Actualizar el intervalo de comprobación de una URL ya registrada.
- /list — Listar las URLs registradas para el chat.
- /remove <url> — Eliminar una URL registrada.
- /help — Mostrar ayuda con los comandos.

Whitelist (control de acceso)
---------------------------
El bot permite acceso únicamente a chats presentes en la tabla Whitelist de la base de datos. Existe un servicio de seed que inserta entradas iniciales (ver Services/WhitelistSeedService.cs). Para autorizar un chat, añade su ChatId a la tabla Whitelist.

Almacenamiento y migraciones
---------------------------
- EF Core está configurado con WebMonitorContext (Data/EF/WebMonitorContext.cs).
- Las entidades principales son:
  - MonitoringUrls (tabla MonitoringUrls)
  - Whitelist (tabla Whitelist)
- Al iniciar la app, Program.cs llama a db.Database.Migrate() para aplicar migraciones pendientes.
- Para crear/gestionar migraciones localmente (si quieres modificarlas):

  dotnet tool install --global dotnet-ef
  dotnet ef migrations add <Nombre> --project WebMonitorBot --startup-project WebMonitorBot
  dotnet ef database update --project WebMonitorBot --startup-project WebMonitorBot

Cliente LLM
-----------
- Implementación por defecto: DeepseekLlmClient, que usa la librería DeepSeek.Core. Si no se dispone de API key o falla la llamada, el código cae a SimulatedLlmClient que devuelve un resumen sintético.
- Puedes reemplazar ILlmClient por otra implementación (p. ej. Semantic Kernel) registrando otro servicio en DI.

Registro y logging
-------------------
- El proyecto usa ILogger<T> proporcionado por el host genérico de .NET. Ajusta el nivel de logging mediante la configuración de appsettings o el entorno.

Despliegue
---------
- Para producción, asegúrate de:
  - No incluir tokens ni credenciales reales en appsettings.json en el repositorio.
  - Usar variables de entorno o un secreto de la plataforma de despliegue (Azure App Service, contenedor, etc.).
  - Configurar la cadena de conexión con un usuario de base de datos adecuado y cifrado/TrustServerCertificate según tu entorno.
  - Ejecutar con un servicio que supervise la disponibilidad del proceso (systemd, servicio Windows, contenedor orquestado).

Seguridad / buenas prácticas
---------------------------
- Restrinje el acceso al token del bot de Telegram y a la API de Deepseek.
- Usa conexiones cifradas a SQL Server y credenciales gestionadas por el proveedor de despliegue cuando sea posible.

Contribuir
----------
- Pull requests bien descritos y cambios pequeños son bienvenidos.
- Mantén el estilo y convenciones del repositorio (C# idiomático, inyección de dependencias, uso de ILogger).