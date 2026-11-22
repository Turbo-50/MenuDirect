using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Database;
using MenuDirect.Data.Entities;
using MenuDirect.Data.Interfaces;
using Npgsql;

namespace MenuDirect.Data.Repositories;

public class CarritoRepository : ICarritoRepository
{
    private readonly PostgresConnectionFactory _connectionFactory;

    public CarritoRepository(PostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CarritoEntity> ObtenerOCrearCarritoPorMeseroAsync(int idMesero)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();

        const string selectSql = """
            SELECT id_carrito, id_mesero, fecha_creacion, total_general
            FROM carritos
            WHERE id_mesero = @idMesero
            LIMIT 1;
            """;

        await using (var selectCmd = new NpgsqlCommand(selectSql, conn))
        {
            selectCmd.Parameters.AddWithValue("@idMesero", idMesero);
            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CarritoEntity
                {
                    IdCarrito = reader.GetInt32(0),
                    IdMesero = reader.GetInt32(1),
                    FechaCreacion = reader.GetDateTime(2),
                    TotalGeneral = reader.GetDecimal(3)
                };
            }
        }

        const string insertSql = """
            INSERT INTO carritos (id_mesero, fecha_creacion, total_general)
            VALUES (@idMesero, CURRENT_TIMESTAMP, 0.00)
            RETURNING id_carrito, id_mesero, fecha_creacion, total_general;
            """;

        await using var insertCmd = new NpgsqlCommand(insertSql, conn);
        insertCmd.Parameters.AddWithValue("@idMesero", idMesero);
        await using var insertReader = await insertCmd.ExecuteReaderAsync();
        await insertReader.ReadAsync();

        return new CarritoEntity
        {
            IdCarrito = insertReader.GetInt32(0),
            IdMesero = insertReader.GetInt32(1),
            FechaCreacion = insertReader.GetDateTime(2),
            TotalGeneral = insertReader.GetDecimal(3)
        };
    }

    public async Task<List<DetalleCarritoEntity>> ObtenerDetallesCarritoAsync(int idCarrito)
    {
        var lista = new List<DetalleCarritoEntity>();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT id_detalle_carrito, id_carrito, id_producto, cantidad, precio_unitario, subtotal
            FROM detalle_carrito
            WHERE id_carrito = @idCarrito
            ORDER BY id_detalle_carrito ASC;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idCarrito", idCarrito);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new DetalleCarritoEntity
            {
                IdDetalleCarrito = reader.GetInt32(0),
                IdCarrito = reader.GetInt32(1),
                IdProducto = reader.GetInt32(2),
                Cantidad = reader.GetInt32(3),
                PrecioUnitario = reader.GetDecimal(4),
                Subtotal = reader.GetDecimal(5)
            });
        }

        return lista;
    }

    public async Task GuardarOActualizarDetalleAsync(int idCarrito, int idProducto, int cantidad, decimal precioUnitario)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        decimal subtotal = cantidad * precioUnitario;

        const string upsertSql = """
            INSERT INTO detalle_carrito (id_carrito, id_producto, cantidad, precio_unitario, subtotal)
            VALUES (@idCarrito, @idProducto, @cantidad, @precioUnitario, @subtotal)
            ON CONFLICT (id_carrito, id_producto)
            DO UPDATE SET
                cantidad = EXCLUDED.cantidad,
                precio_unitario = EXCLUDED.precio_unitario,
                subtotal = EXCLUDED.subtotal;
            """;

        await using (var cmd = new NpgsqlCommand(upsertSql, conn, tx))
        {
            cmd.Parameters.AddWithValue("@idCarrito", idCarrito);
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.Parameters.AddWithValue("@cantidad", cantidad);
            cmd.Parameters.AddWithValue("@precioUnitario", precioUnitario);
            cmd.Parameters.AddWithValue("@subtotal", subtotal);
            await cmd.ExecuteNonQueryAsync();
        }

        await RecalcularTotalCarritoAsync(conn, tx, idCarrito);
        await tx.CommitAsync();
    }

    public async Task EliminarDetalleProductoAsync(int idCarrito, int idProducto)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        const string deleteSql = """
            DELETE FROM detalle_carrito
            WHERE id_carrito = @idCarrito
              AND id_producto = @idProducto;
            """;

        await using (var cmd = new NpgsqlCommand(deleteSql, conn, tx))
        {
            cmd.Parameters.AddWithValue("@idCarrito", idCarrito);
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            await cmd.ExecuteNonQueryAsync();
        }

        await RecalcularTotalCarritoAsync(conn, tx, idCarrito);
        await tx.CommitAsync();
    }

    public async Task VaciarCarritoAsync(int idCarrito)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        const string sql = """
            DELETE FROM detalle_carrito WHERE id_carrito = @idCarrito;
            UPDATE carritos SET total_general = 0.00 WHERE id_carrito = @idCarrito;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@idCarrito", idCarrito);
        await cmd.ExecuteNonQueryAsync();

        await tx.CommitAsync();
    }

    private static async Task RecalcularTotalCarritoAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int idCarrito)
    {
        const string updateTotalSql = """
            UPDATE carritos
            SET total_general = COALESCE(
                (SELECT SUM(subtotal) FROM detalle_carrito WHERE id_carrito = @idCarrito),
                0.00
            )
            WHERE id_carrito = @idCarrito;
            """;

        await using var cmd = new NpgsqlCommand(updateTotalSql, conn, tx);
        cmd.Parameters.AddWithValue("@idCarrito", idCarrito);
        await cmd.ExecuteNonQueryAsync();
    }
}