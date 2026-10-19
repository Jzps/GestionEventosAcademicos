namespace ApiGestionAcademico.Excepciones;

/// <summary>
/// El usuario no está autenticado o las credenciales son incorrectas.
/// Se traduce a una respuesta HTTP 401.
/// </summary>
public class ExcepcionNoAutorizado : Exception
{
    public ExcepcionNoAutorizado(string mensaje) : base(mensaje)
    {
    }
}
