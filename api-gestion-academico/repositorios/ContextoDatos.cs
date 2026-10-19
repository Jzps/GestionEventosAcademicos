using ApiGestionAcademico.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiGestionAcademico.Repositorios;

/// <summary>
/// Contexto de datos de Entity Framework Core. Configura la jerarquía de usuarios
/// (tabla única con discriminador), las restricciones del modelo y las relaciones
/// del diagrama de clases.
/// </summary>
public class ContextoDatos : DbContext
{
    public ContextoDatos(DbContextOptions<ContextoDatos> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();
    public DbSet<Asistencia> Asistencias => Set<Asistencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfiguracionUsuarios(modelBuilder);
        ConfiguracionCategorias(modelBuilder);
        ConfiguracionEventos(modelBuilder);
        ConfiguracionInscripciones(modelBuilder);
        ConfiguracionAsistencias(modelBuilder);
    }

    private static void ConfiguracionUsuarios(ModelBuilder modelBuilder)
    {
        var usuario = modelBuilder.Entity<Usuario>();
        usuario.ToTable("Usuarios", tabla =>
        {
            tabla.HasCheckConstraint("CK_Usuarios_Rol", "[Rol] IN ('Estudiante', 'Responsable')");
            tabla.HasCheckConstraint(
                "CK_Usuarios_Perfil",
                "([Rol] = 'Estudiante' AND [Codigo] IS NOT NULL AND [Programa] IS NOT NULL AND [Dependencia] IS NULL AND [Cargo] IS NULL) " +
                "OR ([Rol] = 'Responsable' AND [Codigo] IS NULL AND [Programa] IS NULL AND [Dependencia] IS NOT NULL AND [Cargo] IS NOT NULL)");
        });

        usuario.HasKey(u => u.Id);
        usuario.Property(u => u.Id).ValueGeneratedOnAdd();
        usuario.Property(u => u.NombreCompleto).HasMaxLength(200).IsRequired();
        usuario.Property(u => u.Email).HasMaxLength(200).IsRequired();
        usuario.Property(u => u.ContrasenaHash).HasMaxLength(400).IsRequired();
        usuario.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20).IsRequired();
        usuario.Property(u => u.Activo).IsRequired();
        usuario.HasIndex(u => u.Email).IsUnique();

        // La columna Rol es el discriminador de la jerarquía: garantiza que el tipo
        // concreto del usuario y su rol nunca se contradigan (LSP).
        usuario.HasDiscriminator<RolUsuario>("Rol")
            .HasValue<Estudiante>(RolUsuario.Estudiante)
            .HasValue<Responsable>(RolUsuario.Responsable);

        var estudiante = modelBuilder.Entity<Estudiante>();
        estudiante.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
        estudiante.Property(e => e.Programa).HasMaxLength(150).IsRequired();

        var responsable = modelBuilder.Entity<Responsable>();
        responsable.Property(r => r.Dependencia).HasMaxLength(150).IsRequired();
        responsable.Property(r => r.Cargo).HasMaxLength(150).IsRequired();
    }

    private static void ConfiguracionCategorias(ModelBuilder modelBuilder)
    {
        var categoria = modelBuilder.Entity<Categoria>();
        categoria.ToTable("Categorias");
        categoria.HasKey(c => c.Id);
        categoria.Property(c => c.Id).ValueGeneratedOnAdd();
        categoria.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
        categoria.Property(c => c.Descripcion).HasMaxLength(500);
        categoria.HasIndex(c => c.Nombre).IsUnique();
    }

    private static void ConfiguracionEventos(ModelBuilder modelBuilder)
    {
        var evento = modelBuilder.Entity<Evento>();
        evento.ToTable("Eventos", tabla =>
        {
            tabla.HasCheckConstraint("CK_Eventos_Cupos", "[CuposTotales] > 0");
            tabla.HasCheckConstraint("CK_Eventos_Fechas", "[FechaHoraFin] > [FechaHoraInicio]");
        });
        evento.HasKey(e => e.Id);
        evento.Property(e => e.Id).ValueGeneratedOnAdd();
        evento.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
        evento.Property(e => e.Descripcion).HasMaxLength(2000).IsRequired();
        evento.Property(e => e.Lugar).HasMaxLength(200).IsRequired();
        evento.Property(e => e.Estado).HasConversion<int>().IsRequired();
        evento.Property(e => e.CuposTotales).IsRequired();
        evento.Property(e => e.Version).IsRowVersion();
        evento.HasIndex(e => e.ResponsableId);
        evento.HasIndex(e => e.CategoriaId);
        evento.HasIndex(e => e.FechaHoraInicio);

        evento.HasOne(e => e.Responsable)
            .WithMany()
            .HasForeignKey(e => e.ResponsableId)
            .OnDelete(DeleteBehavior.Restrict);

        evento.HasOne(e => e.Categoria)
            .WithMany()
            .HasForeignKey(e => e.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfiguracionInscripciones(ModelBuilder modelBuilder)
    {
        var inscripcion = modelBuilder.Entity<Inscripcion>();
        inscripcion.ToTable("Inscripciones");
        inscripcion.HasKey(i => i.Id);
        inscripcion.Property(i => i.Id).ValueGeneratedOnAdd();
        inscripcion.Property(i => i.Estado).HasConversion<int>().IsRequired();
        inscripcion.HasIndex(i => new { i.EstudianteId, i.EventoId })
            .IsUnique()
            .HasFilter("[Estado] = 1")
            .HasDatabaseName("UX_Inscripciones_Activas");
        inscripcion.HasIndex(i => new { i.EventoId, i.Estado }).HasDatabaseName("IX_Inscripciones_Evento");

        inscripcion.HasOne(i => i.Estudiante)
            .WithMany()
            .HasForeignKey(i => i.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);

        inscripcion.HasOne(i => i.Evento)
            .WithMany(e => e.Inscripciones)
            .HasForeignKey(i => i.EventoId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfiguracionAsistencias(ModelBuilder modelBuilder)
    {
        var asistencia = modelBuilder.Entity<Asistencia>();
        asistencia.ToTable("Asistencias");
        asistencia.HasKey(a => a.Id);
        asistencia.Property(a => a.Id).ValueGeneratedOnAdd();
        asistencia.Property(a => a.Asistio).IsRequired();
        asistencia.Property(a => a.Observacion).HasMaxLength(500);
        asistencia.HasIndex(a => a.InscripcionId).IsUnique().HasDatabaseName("UX_Asistencias_Inscripcion");

        asistencia.HasOne(a => a.Inscripcion)
            .WithOne(i => i.Asistencia)
            .HasForeignKey<Asistencia>(a => a.InscripcionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
