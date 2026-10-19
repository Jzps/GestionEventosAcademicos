namespace ApiGestionAcademico.Modelos;

/// <summary>Clasificación temática de los eventos académicos.</summary>
public sealed class Categoria
{
    /// <summary>Identificador de la categoría.</summary>
    public long Id { get; internal set; }

    /// <summary>Nombre de la categoría.</summary>
    public string Nombre { get; internal set; } = string.Empty;

    /// <summary>Descripción de la categoría.</summary>
    public string Descripcion { get; internal set; } = string.Empty;

    public Categoria()
    {
    }

    public Categoria(string nombre, string descripcion)
    {
        Nombre = nombre;
        Descripcion = descripcion;
    }
}
