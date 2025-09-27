using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace StayGo.Data;

public class StayGoContextFactory : IDesignTimeDbContextFactory<StayGoContext>
{
    public StayGoContext CreateDbContext(string[] args)
    {
        // Obtiene la configuración desde appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Obtiene la cadena de conexión
        var connectionString = configuration.GetConnectionString("StayGoContext");

        // Crea las opciones del DbContext
        var builder = new DbContextOptionsBuilder<StayGoContext>();
        builder.UseSqlite(connectionString);

        // Retorna una nueva instancia del contexto
        return new StayGoContext(builder.Options);
    }
}