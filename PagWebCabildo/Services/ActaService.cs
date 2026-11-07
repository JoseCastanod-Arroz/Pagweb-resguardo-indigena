using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IActaService"/> sobre <see cref="AppDbContext"/>.
/// Concentra el CRUD de actas, el orden de listado del más reciente al más
/// antiguo y la coordinación del adjunto en <c>uploads/actas</c> a través de
/// <see cref="IArchivoService"/> (Requisitos 3.1, 3.2, 3.3, 3.4, 4.1, 4.2).
/// </summary>
public sealed class ActaService : IActaService
{
    /// <summary>Subcarpeta de <c>wwwroot/uploads</c> donde se almacenan los adjuntos de actas.</summary>
    private const string Subcarpeta = "actas";

    private readonly AppDbContext _contexto;
    private readonly IArchivoService _archivos;

    /// <summary>
    /// Crea el servicio con el contexto de datos y el servicio de archivos inyectados.
    /// </summary>
    /// <param name="contexto">Contexto de EF Core del sitio del resguardo.</param>
    /// <param name="archivos">Servicio transversal de manejo de archivos subidos.</param>
    public ActaService(AppDbContext contexto, IArchivoService archivos)
    {
        _contexto = contexto;
        _archivos = archivos;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Acta>> Listar()
    {
        // Orden "reciente primero": por fecha de asamblea descendente y, a igualdad
        // de fecha, por Id descendente como desempate estable (Requisito 3.1).
        return await _contexto.Actas
            .Include(a => a.RegistradoPor)
            .OrderByDescending(a => a.FechaAsamblea)
            .ThenByDescending(a => a.Id)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Acta?> Obtener(int id)
    {
        return await _contexto.Actas
            .Include(a => a.RegistradoPor)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <inheritdoc />
    public async Task<Acta> Crear(Acta acta, IFormFile archivo)
    {
        ArgumentNullException.ThrowIfNull(acta);

        ValidarCampos(acta);
        await ValidarConsecutivoUnico(acta.Consecutivo, idExcluido: null);

        if (archivo is null || archivo.Length == 0)
        {
            throw new ArgumentException("El archivo del acta es obligatorio.");
        }

        // El guardado valida la extensión permitida y lanza ArgumentException ante
        // una extensión no admitida; se deja propagar hacia el formulario.
        acta.ArchivoRuta = await _archivos.Guardar(archivo, Subcarpeta);
        acta.FechaRegistro = DateTime.Now;

        _contexto.Actas.Add(acta);
        await _contexto.SaveChangesAsync();
        return acta;
    }

    /// <inheritdoc />
    public async Task<Acta?> Editar(Acta acta, IFormFile? archivo)
    {
        ArgumentNullException.ThrowIfNull(acta);

        var existente = await _contexto.Actas.FindAsync(acta.Id);
        if (existente is null)
        {
            return null;
        }

        ValidarCampos(acta);
        await ValidarConsecutivoUnico(acta.Consecutivo, idExcluido: acta.Id);

        existente.Consecutivo = acta.Consecutivo;
        existente.FechaAsamblea = acta.FechaAsamblea;
        existente.Tipo = acta.Tipo;
        existente.Titulo = acta.Titulo;
        existente.Descripcion = acta.Descripcion;

        // Reemplazo de adjunto: si llega un archivo nuevo, se guarda y se elimina el
        // anterior; si no, se conserva la ruta existente.
        if (archivo is not null && archivo.Length > 0)
        {
            var rutaAnterior = existente.ArchivoRuta;
            existente.ArchivoRuta = await _archivos.Guardar(archivo, Subcarpeta);

            if (!string.IsNullOrWhiteSpace(rutaAnterior))
            {
                _archivos.Eliminar(rutaAnterior);
            }
        }

        await _contexto.SaveChangesAsync();
        return existente;
    }

    /// <inheritdoc />
    public async Task<ArchivoAbierto?> AbrirArchivo(int id)
    {
        var acta = await _contexto.Actas.FindAsync(id);
        if (acta is null || string.IsNullOrWhiteSpace(acta.ArchivoRuta))
        {
            return null;
        }

        // Si el archivo físico no existe, IArchivoService.Abrir lanza
        // FileNotFoundException, que el controlador traduce en un 404.
        return _archivos.Abrir(acta.ArchivoRuta);
    }

    /// <summary>
    /// Valida los campos obligatorios del acta (consecutivo positivo, fecha
    /// presente, tipo válido y título no vacío), reuniendo todos los errores en un
    /// único mensaje para el formulario (Requisito 3.2).
    /// </summary>
    /// <param name="acta">Acta a validar.</param>
    /// <exception cref="ArgumentException">Si algún campo no cumple la validación.</exception>
    private static void ValidarCampos(Acta acta)
    {
        var errores = new List<string>();

        if (acta.Consecutivo <= 0)
        {
            errores.Add("El consecutivo debe ser un número positivo.");
        }
        if (acta.FechaAsamblea == default)
        {
            errores.Add("La fecha de la asamblea es obligatoria.");
        }
        if (!Enum.IsDefined(acta.Tipo))
        {
            errores.Add("El tipo de asamblea no es válido.");
        }
        if (string.IsNullOrWhiteSpace(acta.Titulo))
        {
            errores.Add("El título es obligatorio.");
        }

        if (errores.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errores));
        }
    }

    /// <summary>
    /// Verifica que el consecutivo no esté usado por otra acta, para evitar que la
    /// restricción de unicidad de la base de datos aflore como una excepción cruda
    /// y mostrar en su lugar un error de validación comprensible (Requisito 3.2).
    /// </summary>
    /// <param name="consecutivo">Consecutivo a comprobar.</param>
    /// <param name="idExcluido">Id del acta que se edita (se excluye de la búsqueda), o <c>null</c> al crear.</param>
    /// <exception cref="ArgumentException">Si el consecutivo ya pertenece a otra acta.</exception>
    private async Task ValidarConsecutivoUnico(int consecutivo, int? idExcluido)
    {
        var duplicado = await _contexto.Actas
            .AnyAsync(a => a.Consecutivo == consecutivo && (idExcluido == null || a.Id != idExcluido));

        if (duplicado)
        {
            throw new ArgumentException($"Ya existe un acta con el consecutivo {consecutivo}.");
        }
    }
}
