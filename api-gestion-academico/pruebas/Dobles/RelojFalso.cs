namespace Pruebas.ApiGestionAcademico.Dobles;

/// <summary>Reloj controlado para las pruebas, de modo que las fechas no dependan del momento real.</summary>
public sealed class RelojFalso : TimeProvider
{
    public RelojFalso(DateTimeOffset instante)
    {
        Instante = instante;
    }

    public DateTimeOffset Instante { get; set; }

    public override DateTimeOffset GetUtcNow() => Instante;
}
