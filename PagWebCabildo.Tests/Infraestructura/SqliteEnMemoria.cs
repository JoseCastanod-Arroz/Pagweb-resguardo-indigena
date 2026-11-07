using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;

namespace PagWebCabildo.Tests.Infraestructura;

/// <summary>
/// Utilidad compartida para pruebas que requieren una base SQLite en memoria.
///
/// Mantiene abierta una única <see cref="SqliteConnection"/> ("DataSource=:memory:")
/// durante toda la vida del objeto. Mientras la conexión permanezca abierta, la base
/// de datos en memoria conserva su contenido, lo que permite crear varios
/// <see cref="AppDbContext"/> independientes (por ejemplo, uno para escribir y otro
/// para releer) sobre el mismo almacén.
/// </summary>
public sealed class SqliteEnMemoria : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<AppDbContext> _opciones;

    public SqliteEnMemoria()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();

        _opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conexion)
            .Options;

        // Crear el esquema a partir del modelo del AppDbContext.
        using var contexto = CrearContexto();
        contexto.Database.EnsureCreated();
    }

    /// <summary>
    /// Crea un nuevo <see cref="AppDbContext"/> que comparte la misma conexión
    /// (y, por tanto, la misma base de datos en memoria).
    /// </summary>
    public AppDbContext CrearContexto() => new AppDbContext(_opciones);

    public void Dispose()
    {
        _conexion.Dispose();
    }
}
