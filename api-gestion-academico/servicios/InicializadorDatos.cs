using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace ApiGestionAcademico.Servicios;

/// <summary>
/// Inicializador local de datos de demostración. Es idempotente: puede ejecutarse en
/// cada arranque de la API sin duplicar cuentas, categorías, eventos ni inscripciones.
/// </summary>
public sealed class InicializadorDatos
{
    /// <summary>Crea la información de demostración si todavía no existe.</summary>
    public async Task SembrarAsync(ContextoDatos contexto, CancellationToken cancellationToken)
    {
        await sembrarCategoriasAsync(contexto, cancellationToken);
        await sembrarUsuariosAsync(contexto, cancellationToken);
        await sembrarEventosAsync(contexto, cancellationToken);
        await sembrarInscripcionesAsync(contexto, cancellationToken);
        await sembrarAsistenciasAsync(contexto, cancellationToken);
    }

    private static async Task sembrarCategoriasAsync(ContextoDatos contexto, CancellationToken cancellationToken)
    {
        var existentes = await contexto.Categorias.Select(c => c.Nombre).ToListAsync(cancellationToken);

        var catalogo = new (string Nombre, string Descripcion)[]
        {
            ("Conferencias", "Encuentros con expertos nacionales e internacionales sobre temas de interés académico."),
            ("Seminarios", "Sesiones de discusión académica orientadas al intercambio de investigación."),
            ("Talleres", "Actividades practicas orientadas al ejercicio de herramientas y metodologias."),
            ("Charlas", "Presentaciones breves de iniciacion o divulgacion sobre un tema especifico."),
            ("Formacion", "Programas de formacion complementaria dirigidos a estudiantes de pregrado.")
        };

        foreach (var (nombre, descripcion) in catalogo)
        {
            if (!existentes.Contains(nombre))
            {
                contexto.Categorias.Add(new Categoria(nombre, descripcion));
            }
        }

        await contexto.SaveChangesAsync(cancellationToken);
    }

    private async Task sembrarUsuariosAsync(ContextoDatos contexto, CancellationToken cancellationToken)
    {
        var hash = new ServicioHashContrasena();

        var estudiantes = new (string Email, string Nombre, string Codigo, string Programa)[]
        {
            ("estudiante@uni.edu.co", "Ana Maria Gomez Restrepo", "EST2024001", "Ingenieria de Sistemas"),
            ("ana.gomez@uni.edu.co", "Daniela Torres Mejia", "EST2024002", "Ingenieria de Software"),
            ("juan.perez@uni.edu.co", "Juan Esteban Perez", "EST2024003", "Contaduria Publica"),
            ("laura.torres@uni.edu.co", "Laura Valentina Torres", "EST2024004", "Administracion de Empresas")
        };

        var responsables = new (string Email, string Nombre, string Dependencia, string Cargo)[]
        {
            ("responsable@uni.edu.co", "Carlos Alberto Rios", "Facultad de Ingenieria", "Coordinador de Eventos Academicos"),
            ("coordinacion@uni.edu.co", "Maria Fernanda Lopez", "Direccion Academica", "Directora de Formacion")
        };

        var emailsExistentes = await contexto.Usuarios.Select(u => u.Email).ToListAsync(cancellationToken);

        foreach (var (email, nombre, codigo, programa) in estudiantes)
        {
            if (!emailsExistentes.Contains(email))
            {
                contexto.Usuarios.Add(new Estudiante(nombre, email, hash.Generar("Estudiante2024!"), codigo, programa));
            }
        }

        foreach (var (email, nombre, dependencia, cargo) in responsables)
        {
            if (!emailsExistentes.Contains(email))
            {
                contexto.Usuarios.Add(new Responsable(nombre, email, hash.Generar("Responsable2024!"), dependencia, cargo));
            }
        }

        await contexto.SaveChangesAsync(cancellationToken);
    }

    private async Task sembrarEventosAsync(ContextoDatos contexto, CancellationToken cancellationToken)
    {
        if (await contexto.Eventos.AnyAsync(cancellationToken))
        {
            return;
        }

        var categorias = await contexto.Categorias.ToDictionaryAsync(c => c.Nombre, c => c, cancellationToken);
        var responsables = await contexto.Usuarios.OfType<Responsable>().ToListAsync(cancellationToken);
        var principal = responsables.FirstOrDefault(r => r.Email == "responsable@uni.edu.co");
        var secundario = responsables.FirstOrDefault(r => r.Email == "coordinacion@uni.edu.co");

        if (principal is null || secundario is null)
        {
            return;
        }

        var ahora = DateTime.Now;
        var eventos = new (string Nombre, string Descripcion, string Categoria, int Dias, int Cupos, DateTime? HoraInicio, DateTime? HoraFin, Responsable Responsable)[]
        {
            ("Seminario de Arquitectura de Software",
             "Seminario academico donde se bilateral el analisis, el diseno y la evolucion de sistemas de software en contextos reales de la industria y la academia.",
             "Seminarios", 3, 40, new DateTime(2024, 1, 1, 10, 0, 0), new DateTime(2024, 1, 1, 12, 0, 0), principal),
            ("Taller de Bases de Datos Relacionales",
             "Taller practico sobre modelado relacional, normalizacion, indices y optimizacion de consultas sobre SQL Server.",
             "Talleres", 7, 25, new DateTime(2024, 1, 1, 14, 0, 0), new DateTime(2024, 1, 1, 17, 0, 0), principal),
            ("Conferencia de Ciberseguridad en la Institucion",
             "Conferencia sobre aplicacion de normas de seguridad de la informacion, gestion de riesgos y cultura organizacional en entornos universitarios.",
             "Conferencias", 14, 100, new DateTime(2024, 1, 1, 9, 0, 0), new DateTime(2024, 1, 1, 11, 0, 0), principal),
            ("Charla: Linea de Tiempo de la Investigacion Academica",
             "Charla introductoria que recorre las etapas de un proyecto de investigacion desde la formulacion del problema hasta la difusion de resultados.",
             "Charlas", 21, 60, new DateTime(2024, 1, 1, 16, 0, 0), new DateTime(2024, 1, 1, 17, 30, 0), secundario),
            ("Seminario de Metodologias de Investigacion Aplicada",
             "Seminario intensivo sobre diseno de investigations, instrumentos de recoleccion y analisis de datos en proyectos aplicados.",
             "Seminarios", 2, 2, new DateTime(2024, 1, 1, 8, 0, 0), new DateTime(2024, 1, 1, 10, 0, 0), principal),
            ("Taller de Prototipado de Interfaces",
             "Taller con cupo agotado, usado para demostrar el control de disponibilidad de cupos del sistema.",
             "Talleres", 5, 2, new DateTime(2024, 1, 1, 9, 0, 0), new DateTime(2024, 1, 1, 12, 0, 0), principal),
            ("Taller de Metodologias Agiles para Proyectos de Software",
             "Evento finalizado, usado para demostrar la consulta del registro de asistencia de un evento ya realizado.",
             "Talleres", -10, 30, new DateTime(2024, 1, 1, 8, 0, 0), new DateTime(2024, 1, 1, 12, 0, 0), principal),
            ("Conferencia de Salud Mental en la Universidad",
             "Evento con cupo agotado, usado para demostrar el control de disponibilidad de cupos del sistema.",
             "Conferencias", -3, 1, new DateTime(2024, 1, 1, 15, 0, 0), new DateTime(2024, 1, 1, 17, 0, 0), principal)
        };

        foreach (var (nombre, descripcion, categoria, dias, cupos, horaInicio, horaFin, responsable) in eventos)
        {
            var inicio = ahora.AddDays(dias).Date.Add(horaInicio!.Value.TimeOfDay);
            var fin = ahora.AddDays(dias).Date.Add(horaFin!.Value.TimeOfDay);

            // La fecha de registro se situa siempre antes del inicio del evento (RN-03).
            var fechaRegistro = inicio.AddDays(-15);

            var evento = new EventoBuilder()
                .ConNombre(nombre)
                .ConDescripcion(descripcion)
                .ConFechas(inicio, fin)
                .ConLugar("Auditorio Principal - Edificio A")
                .ConCupos(cupos)
                .ConCategoria(categorias[categoria])
                .ConResponsable(responsable)
                .ConFechaRegistro(fechaRegistro)
                .Build();

            evento.Estado = dias < 0 ? EstadoEvento.Finalizado : EstadoEvento.Programado;
            contexto.Eventos.Add(evento);
        }

        await contexto.SaveChangesAsync(cancellationToken);
    }

    private async Task sembrarInscripcionesAsync(ContextoDatos contexto, CancellationToken cancellationToken)
    {
        if (await contexto.Inscripciones.AnyAsync(cancellationToken))
        {
            return;
        }

        var estudiantes = await contexto.Usuarios.OfType<Estudiante>().ToListAsync(cancellationToken);
        var eventos = await contexto.Eventos.ToListAsync(cancellationToken);

        var ana = estudiantes.FirstOrDefault(e => e.Email == "estudiante@uni.edu.co");
        var daniela = estudiantes.FirstOrDefault(e => e.Email == "ana.gomez@uni.edu.co");
        var juan = estudiantes.FirstOrDefault(e => e.Email == "juan.perez@uni.edu.co");
        var laura = estudiantes.FirstOrDefault(e => e.Email == "laura.torres@uni.edu.co");

        var seminario = eventos.FirstOrDefault(e => e.Nombre.StartsWith("Seminario de Arquitectura"));
        var taller = eventos.FirstOrDefault(e => e.Nombre.StartsWith("Taller de Bases"));
        var prototipado = eventos.FirstOrDefault(e => e.Nombre.StartsWith("Taller de Prototipado"));
        var agile = eventos.FirstOrDefault(e => e.Nombre.StartsWith("Taller de Metodologias Agiles"));
        var salud = eventos.FirstOrDefault(e => e.Nombre.StartsWith("Conferencia de Salud"));

        var plan = new List<(Estudiante? Estudiante, Evento? Evento, DateTime Fecha)>
        {
            (ana, seminario, DateTime.Now.AddDays(-2)),
            (daniela, seminario, DateTime.Now.AddDays(-1)),
            (juan, seminario, DateTime.Now.AddDays(-1)),
            (ana, taller, DateTime.Now.AddHours(-6)),
            (daniela, taller, DateTime.Now.AddHours(-5)),
            (laura, taller, DateTime.Now.AddHours(-3)),
            (daniela, prototipado, DateTime.Now.AddHours(-4)),
            (juan, prototipado, DateTime.Now.AddHours(-2)),
            (ana, agile, DateTime.Now.AddDays(-12)),
            (daniela, agile, DateTime.Now.AddDays(-12)),
            (juan, agile, DateTime.Now.AddDays(-11)),
            (ana, salud, DateTime.Now.AddDays(-4))
        };

        foreach (var (estudiante, evento, fecha) in plan)
        {
            if (estudiante is null || evento is null)
            {
                continue;
            }

            var inscripcion = new Inscripcion(evento, estudiante, fecha);
            evento.Inscripciones.Add(inscripcion);
            contexto.Inscripciones.Add(inscripcion);
        }

        await contexto.SaveChangesAsync(cancellationToken);
    }

    private async Task sembrarAsistenciasAsync(ContextoDatos contexto, CancellationToken cancellationToken)
    {
        if (await contexto.Asistencias.AnyAsync(cancellationToken))
        {
            return;
        }

        var eventoAgil = await contexto.Eventos.FirstOrDefaultAsync(e => e.Nombre.StartsWith("Taller de Metodologias Agiles"), cancellationToken);
        if (eventoAgil is null)
        {
            return;
        }

        var inscripciones = await contexto.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i => i.EventoId == eventoAgil.Id)
            .ToListAsync(cancellationToken);

        var indice = 0;
        foreach (var inscripcion in inscripciones)
        {
            var asistio = indice % 3 != 2;
            var asistencia = new Asistencia(inscripcion, asistio, asistio ? null : "No se presented al evento", eventoAgil.FechaHoraFin);
            contexto.Asistencias.Add(asistencia);
            inscripcion.Asistencia = asistencia;
            indice++;
        }

        await contexto.SaveChangesAsync(cancellationToken);
    }
}
