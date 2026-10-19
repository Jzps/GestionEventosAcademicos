namespace ApiGestionAcademico.Excepciones;

/// <summary>
/// Error de negocio: la operación es válida en la forma pero viola una regla del dominio
/// (por ejemplo, no hay cupos disponibles o el evento no admite inscripciones).
/// Se traduce a una respuesta HTTP 409 en el manejador centralizado de errores.
/// </summary>
public class ExcepcionNegocio : Exception
{
    public ExcepcionNegocio(string mensaje) : base(mensaje)
    {
    }

    public ExcepcionNegocio(string mensaje, Exception innerException) : base(mensaje, innerException)
    {
    }
}
