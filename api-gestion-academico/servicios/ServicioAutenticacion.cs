using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace ApiGestionAcademico.Servicios;

/// <summary>
/// Servicio de autenticación local del prototipo: valida las credenciales de los usuarios
/// almacenados en SQL Server y emite el token de sesión.
/// </summary>
public sealed class ServicioAutenticacion
{
    private readonly IRepositorioUsuario _usuarios;
    private readonly ServicioHashContrasena _hash;
    private readonly ServicioToken _token;

    public ServicioAutenticacion(IRepositorioUsuario usuarios, ServicioHashContrasena hash, ServicioToken token)
    {
        _usuarios = usuarios;
        _hash = hash;
        _token = token;
    }

    /// <summary>Autentica a un usuario y devuelve su token y sus datos.</summary>
    public async Task<RespuestaAutenticacionDto> IniciarSesionAsync(PeticionLogin peticion, CancellationToken cancellationToken)
    {
        var email = peticion.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarios.ObtenerPorEmailAsync(email, cancellationToken);

        // Se usa el mismo mensaje para usuario inexistente y contraseña incorrecta
        // para no revelar qué cuentas existen.
        if (usuario is null || !_hash.Verificar(peticion.Contrasena, usuario.ContrasenaHash))
        {
            throw new ExcepcionNoAutorizado("Las credenciales ingresadas no son correctas.");
        }

        if (!usuario.Activo)
        {
            throw new ExcepcionNoAutorizado("La cuenta se encuentra inactiva.");
        }

        var (token, expira) = _token.Generar(usuario);

        return new RespuestaAutenticacionDto
        {
            Token = token,
            ExpiraEn = expira,
            Usuario = Mapear(usuario)
        };
    }

    /// <summary>
    /// Registra una cuenta pública de estudiante y devuelve su token para iniciar sesión.
    /// El rol lo asigna siempre el backend: el cliente no puede enviarlo.
    /// </summary>
    public async Task<RespuestaAutenticacionDto> RegistrarAsync(PeticionRegistroDto peticion, CancellationToken cancellationToken)
    {
        var email = peticion.Email.Trim().ToLowerInvariant();

        var existente = await _usuarios.ObtenerPorEmailAsync(email, cancellationToken);
        if (existente is not null)
        {
            throw new ExcepcionNegocio("Ya existe una cuenta registrada con ese correo.");
        }

        if (!CumplePoliticaContrasena(peticion.Contrasena))
        {
            throw new ExcepcionNegocio("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número.");
        }

        // El rol se fija en el backend: la petición no puede influir en el tipo de cuenta.
        var estudiante = new Estudiante(
            peticion.NombreCompleto.Trim(),
            email,
            _hash.Generar(peticion.Contrasena),
            peticion.Codigo.Trim(),
            peticion.Programa.Trim());

        try
        {
            await _usuarios.GuardarAsync(estudiante, cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Carrera entre dos registros con el mismo correo: el índice único lo impide.
            throw new ExcepcionNegocio("Ya existe una cuenta registrada con ese correo.");
        }

        var (token, expira) = _token.Generar(estudiante);

        return new RespuestaAutenticacionDto
        {
            Token = token,
            ExpiraEn = expira,
            Usuario = Mapear(estudiante)
        };
    }

    /// <summary>Política mínima de contraseñas para el registro público del prototipo.</summary>
    private static bool CumplePoliticaContrasena(string contrasena)
        => !string.IsNullOrWhiteSpace(contrasena)
           && contrasena.Length >= 8
           && contrasena.Any(char.IsUpper)
           && contrasena.Any(char.IsLower)
           && contrasena.Any(char.IsDigit);

    /// <summary>Obtiene los datos del usuario autenticado.</summary>
    public async Task<UsuarioActualDto> ObtenerPerfilAsync(long usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("El usuario no existe.");

        return Mapear(usuario);
    }

    private static UsuarioActualDto Mapear(Usuario usuario) => new()
    {
        Id = usuario.Id,
        NombreCompleto = usuario.NombreCompleto,
        Email = usuario.Email,
        Rol = usuario.Rol,
        RolDescripcion = usuario.Rol == RolUsuario.Estudiante ? "Estudiante" : "Responsable",
        Codigo = (usuario as Estudiante)?.Codigo,
        Programa = (usuario as Estudiante)?.Programa,
        Dependencia = (usuario as Responsable)?.Dependencia,
        Cargo = (usuario as Responsable)?.Cargo
    };
}
