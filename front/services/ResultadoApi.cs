namespace FrontGestionEventos.Services;

/// <summary>
/// Resultado de una llamada a la API. Encapsula el éxito, los datos y el mensaje de
/// error para que las páginas puedan mostrarlos sin conocer detalles de HTTP.
/// </summary>
public sealed class ResultadoApi<T>
{
    private ResultadoApi(bool exito, T? datos, string mensaje, int codigo)
    {
        Exito = exito;
        Datos = datos;
        Mensaje = mensaje;
        Codigo = codigo;
    }

    public bool Exito { get; }

    public T? Datos { get; }

    public string Mensaje { get; }

    public int Codigo { get; }

    public static ResultadoApi<T> Exitoso(T? datos) => new(true, datos, string.Empty, 200);

    public static ResultadoApi<T> Error(string mensaje, int codigo = 0) => new(false, default, mensaje, codigo);
}
