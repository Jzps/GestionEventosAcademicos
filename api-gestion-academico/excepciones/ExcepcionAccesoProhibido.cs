namespace ApiGestionAcademico.Excepciones;

/// <summary>
/// El usuario está autenticado pero no tiene permisos para la operación (por ejemplo,
/// un responsable que intenta consultar participantes de un evento que no administra).
/// Se traduce a una respuesta HTTP 403.
/// </summary>
public class ExcepcionAccesoProhibido : Exception
{
    public ExcepcionAccesoProhibido(string mensaje) : base(mensaje)
    {
    }
}
