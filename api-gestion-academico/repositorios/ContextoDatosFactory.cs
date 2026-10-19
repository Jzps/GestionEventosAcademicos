using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiGestionAcademico.Repositorios;

/// <summary>
/// Fábrica de diseño utilizada por las herramientas de Entity Framework (migraciones).
/// Solo se usa en tiempo de diseño: en ejecución la configuración llega por
/// inyección de dependencias.
/// </summary>
public sealed class ContextoDatosFactory : IDesignTimeDbContextFactory<ContextoDatos>
{
    public ContextoDatos CreateDbContext(string[] args)
    {
        var opcionBuilder = new DbContextOptionsBuilder<ContextoDatos>();
        var cadena = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer")
            ?? "Server=localhost,1433;Database=GestionEventosAcademicos;User Id=sa;Password=GestionEventos2024!;TrustServerCertificate=True;";

        opcionBuilder.UseSqlServer(cadena);
        return new ContextoDatos(opcionBuilder.Options);
    }
}
