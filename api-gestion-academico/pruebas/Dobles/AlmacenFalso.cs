using ApiGestionAcademico.Modelos;

namespace Pruebas.ApiGestionAcademico.Dobles;

/// <summary>
/// Almacén en memoria que simula la persistencia para las pruebas unitarias.
/// Permite construir los dobles de los repositorios sin depender de SQL Server.
/// </summary>
public sealed class AlmacenFalso
{
    public Dictionary<long, Usuario> Usuarios { get; } = new();
    public Dictionary<long, Evento> Eventos { get; } = new();
    public Dictionary<long, Inscripcion> Inscripciones { get; } = new();
    public Dictionary<long, Asistencia> Asistencias { get; } = new();

    public long SiguienteId { get; set; } = 1;

    public long ObtenerSiguienteId() => SiguienteId++;
}
