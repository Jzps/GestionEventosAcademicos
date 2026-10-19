using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos.Estados;

namespace ApiGestionAcademico.Modelos;

/// <summary>
/// Patrón Builder: construye eventos de forma legible y garantiza que el evento nazca
/// válido. Evita constructores con demasiados parámetros posicionales y centraliza las
/// validaciones RN-03 (fecha de inicio posterior a la de registro), RN-04 (cupos mayores
/// que cero) y la coherencia del rango de fechas.
/// </summary>
public sealed class EventoBuilder
{
    private string? _nombre;
    private string? _descripcion;
    private DateTime? _fechaHoraInicio;
    private DateTime? _fechaHoraFin;
    private string? _lugar;
    private int? _cuposTotales;
    private Categoria? _categoria;
    private Responsable? _responsable;
    private DateTime? _fechaRegistro;

    /// <summary>Asigna el nombre del evento.</summary>
    public EventoBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    /// <summary>Asigna la descripción del evento.</summary>
    public EventoBuilder ConDescripcion(string descripcion)
    {
        _descripcion = descripcion;
        return this;
    }

    /// <summary>Asigna el rango de fechas del evento.</summary>
    public EventoBuilder ConFechas(DateTime fechaHoraInicio, DateTime fechaHoraFin)
    {
        _fechaHoraInicio = fechaHoraInicio;
        _fechaHoraFin = fechaHoraFin;
        return this;
    }

    /// <summary>Asigna el lugar donde se realiza el evento.</summary>
    public EventoBuilder ConLugar(string lugar)
    {
        _lugar = lugar;
        return this;
    }

    /// <summary>Asigna el cupo total del evento.</summary>
    public EventoBuilder ConCupos(int cuposTotales)
    {
        _cuposTotales = cuposTotales;
        return this;
    }

    /// <summary>Asigna la categoría del evento.</summary>
    public EventoBuilder ConCategoria(Categoria categoria)
    {
        _categoria = categoria;
        return this;
    }

    /// <summary>Asigna el responsable que administra el evento.</summary>
    public EventoBuilder ConResponsable(Responsable responsable)
    {
        _responsable = responsable;
        return this;
    }

    /// <summary>Asigna la fecha de registro del evento.</summary>
    public EventoBuilder ConFechaRegistro(DateTime fechaRegistro)
    {
        _fechaRegistro = fechaRegistro;
        return this;
    }

    /// <summary>
    /// Construye el evento validando los datos. Si algún dato es inválido se lanza
    /// <see cref="ExcepcionNegocio"/> y no se crea ninguna entidad.
    /// </summary>
    public Evento Build()
    {
        var nombre = Requerido(_nombre, "El nombre del evento es obligatorio.");
        var descripcion = Requerido(_descripcion, "La descripción del evento es obligatoria.");
        var lugar = Requerido(_lugar, "El lugar del evento es obligatorio.");

        if (_fechaHoraInicio is null)
        {
            throw new ExcepcionNegocio("La fecha y hora de inicio del evento es obligatoria.");
        }

        if (_fechaHoraFin is null)
        {
            throw new ExcepcionNegocio("La fecha y hora de finalización del evento es obligatoria.");
        }

        if (_categoria is null)
        {
            throw new ExcepcionNegocio("Debe seleccionar una categoría para el evento.");
        }

        if (_responsable is null)
        {
            throw new ExcepcionNegocio("El evento debe tener un responsable asignado.");
        }

        if (_cuposTotales is null)
        {
            throw new ExcepcionNegocio("El cupo total del evento es obligatorio.");
        }

        var fechaRegistro = _fechaRegistro ?? DateTime.Now;

        // RN-04: los cupos totales deben ser mayores que cero.
        if (_cuposTotales.Value <= 0)
        {
            throw new ExcepcionNegocio("Los cupos totales deben ser mayores que cero.");
        }

        // RN-03: la fecha de inicio debe ser posterior a la fecha de registro.
        if (_fechaHoraInicio.Value <= fechaRegistro)
        {
            throw new ExcepcionNegocio("La fecha y hora de inicio debe ser posterior a la fecha de registro del evento.");
        }

        if (_fechaHoraFin.Value <= _fechaHoraInicio.Value)
        {
            throw new ExcepcionNegocio("La fecha y hora de finalización debe ser posterior a la fecha de inicio.");
        }

        var evento = new Evento
        {
            Nombre = nombre,
            Descripcion = descripcion,
            Lugar = lugar,
            FechaHoraInicio = _fechaHoraInicio.Value,
            FechaHoraFin = _fechaHoraFin.Value,
            CuposTotales = _cuposTotales.Value,
            FechaRegistro = fechaRegistro,
            CategoriaId = _categoria.Id,
            Categoria = _categoria,
            ResponsableId = _responsable.Id,
            Responsable = _responsable,
            // Estado inicial del ciclo de vida. El dominio lo promociona a
            // "Inscripciones abiertas" de forma automática en la primera lectura.
            Estado = EstadoEvento.Programado
        };

        return evento;
    }

    private static string Requerido(string? valor, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ExcepcionNegocio(mensaje);
        }

        return valor.Trim();
    }
}
