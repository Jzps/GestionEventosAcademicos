namespace ApiGestionAcademico.Excepciones;

/// <summary>El recurso solicitado no existe. Se traduce a una respuesta HTTP 404.</summary>
public class ExcepcionNoEncontrado : Exception
{
    public ExcepcionNoEncontrado(string mensaje) : base(mensaje)
    {
    }
}
