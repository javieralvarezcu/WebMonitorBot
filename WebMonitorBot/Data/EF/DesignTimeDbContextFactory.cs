using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WebMonitorBot.Data.EF
{
    // Provee DbContext en tiempo de diseño para dotnet-ef
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WebMonitorContext>
    {
        public WebMonitorContext CreateDbContext(string[] args)
        {
            // Intentar leer la cadena de conexión de variable de entorno primero
            var conn = Environment.GetEnvironmentVariable("WEBMONITORBOT_CONN");
            if (string.IsNullOrEmpty(conn))
            {
                // Intentar cargar appsettings.json desde el directorio del proyecto
                var basePath = Directory.GetCurrentDirectory();
                var configPath = Path.Combine(basePath, "appsettings.json");
                var builder = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(configPath, optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables();

                var configuration = builder.Build();
                conn = configuration.GetSection("ConnectionStrings")?["Default"];
            }

            if (string.IsNullOrEmpty(conn))
                throw new InvalidOperationException("Connection string not found. Set WEBMONITORBOT_CONN or ConnectionStrings:Default in appsettings.json.");

            var optionsBuilder = new DbContextOptionsBuilder<WebMonitorContext>();
            optionsBuilder.UseSqlServer(conn);

            return new WebMonitorContext(optionsBuilder.Options);
        }
    }
}
