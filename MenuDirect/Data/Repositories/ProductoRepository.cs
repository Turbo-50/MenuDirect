using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Database;
using MenuDirect.Data.Entities;
using MenuDirect.Data.Interfaces;
using Npgsql;

namespace MenuDirect.Data.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly PostgresConnectionFactory _connectionFactory;

    public ProductoRepository(PostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ProductoEntity>> ObtenerTodosDisponiblesAsync()
    {
        var lista = new List<ProductoEntity>();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT id_producto, id_categoria, id_subcategoria, nombre, descripcion, precio, imagen, disponible
            FROM productos
            WHERE disponible = TRUE
            ORDER BY id_producto ASC;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new ProductoEntity
            {
                IdProducto = reader.GetInt32(0),
                IdCategoria = reader.GetInt32(1),
                IdSubcategoria = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Nombre = reader.GetString(3),
                Descripcion = reader.IsDBNull(4) ? null : reader.GetString(4),
                Precio = reader.GetDecimal(5),
                Imagen = reader.IsDBNull(6) ? "dotnet_bot.png" : reader.GetString(6),
                Disponible = reader.GetBoolean(7)
            });
        }

        return lista;
    }

    public async Task<int> CrearProductoAsync(ProductoEntity producto)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            INSERT INTO productos (id_categoria, id_subcategoria, nombre, descripcion, precio, imagen, disponible)
            VALUES (@idCategoria, @idSubcategoria, @nombre, @descripcion, @precio, @imagen, TRUE)
            RETURNING id_producto;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idCategoria", producto.IdCategoria);
        cmd.Parameters.AddWithValue("@idSubcategoria", (object?)producto.IdSubcategoria ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@nombre", producto.Nombre.Trim());
        cmd.Parameters.AddWithValue("@descripcion", (object?)producto.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@precio", producto.Precio);
        cmd.Parameters.AddWithValue("@imagen", string.IsNullOrWhiteSpace(producto.Imagen) ? "dotnet_bot.png" : producto.Imagen);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> EliminarProductoAsync(int idProducto)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            UPDATE productos
            SET disponible = FALSE
            WHERE id_producto = @idProducto;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idProducto", idProducto);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> ModificarPrecioAsync(int idProducto, decimal nuevoPrecio)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            UPDATE productos
            SET precio = @nuevoPrecio
            WHERE id_producto = @idProducto;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nuevoPrecio", nuevoPrecio);
        cmd.Parameters.AddWithValue("@idProducto", idProducto);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> ModificarCategoriaAsync(int idProducto, int idCategoria, int? idSubcategoria)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            UPDATE productos
            SET id_categoria = @idCategoria,
                id_subcategoria = @idSubcategoria
            WHERE id_producto = @idProducto;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
        cmd.Parameters.AddWithValue("@idSubcategoria", (object?)idSubcategoria ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@idProducto", idProducto);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}