using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Database;
using MenuDirect.Data.Entities;
using MenuDirect.Data.Interfaces;
using Npgsql;

namespace MenuDirect.Data.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly PostgresConnectionFactory _connectionFactory;

    public PedidoRepository(PostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CrearPedidoCompletoAsync(
        PedidoEntity pedido,
        List<DetallePedidoEntity> detalles,
        int? idCarritoALimpiar = null)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            const string insertPedidoSql = """
                INSERT INTO pedidos (id_mesero, numero_mesa, notas, estado, fecha_hora, total)
                VALUES (@idMesero, @numeroMesa, @notas, 'PENDIENTE', CURRENT_TIMESTAMP, @total)
                RETURNING id_pedido;
                """;

            int idPedidoCreado;
            await using (var cmdPedido = new NpgsqlCommand(insertPedidoSql, conn, tx))
            {
                cmdPedido.Parameters.AddWithValue("@idMesero", pedido.IdMesero);
                cmdPedido.Parameters.AddWithValue("@numeroMesa", pedido.NumeroMesa);
                cmdPedido.Parameters.AddWithValue("@notas", string.IsNullOrWhiteSpace(pedido.Notas) ? DBNull.Value : pedido.Notas.Trim());
                cmdPedido.Parameters.AddWithValue("@total", pedido.Total);

                idPedidoCreado = Convert.ToInt32(await cmdPedido.ExecuteScalarAsync());
            }

            const string insertDetalleSql = """
                INSERT INTO detalle_pedido (id_pedido, id_producto, cantidad, precio_unitario, subtotal)
                VALUES (@idPedido, @idProducto, @cantidad, @precioUnitario, @subtotal);
                """;

            foreach (var detalle in detalles)
            {
                await using var cmdDet = new NpgsqlCommand(insertDetalleSql, conn, tx);
                cmdDet.Parameters.AddWithValue("@idPedido", idPedidoCreado);
                cmdDet.Parameters.AddWithValue("@idProducto", detalle.IdProducto);
                cmdDet.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                cmdDet.Parameters.AddWithValue("@precioUnitario", detalle.PrecioUnitario);
                cmdDet.Parameters.AddWithValue("@subtotal", detalle.Subtotal);
                await cmdDet.ExecuteNonQueryAsync();
            }

            if (idCarritoALimpiar.HasValue)
            {
                const string clearCartSql = """
                    DELETE FROM detalle_carrito WHERE id_carrito = @idCarrito;
                    UPDATE carritos SET total_general = 0.00 WHERE id_carrito = @idCarrito;
                    """;

                await using var cmdCart = new NpgsqlCommand(clearCartSql, conn, tx);
                cmdCart.Parameters.AddWithValue("@idCarrito", idCarritoALimpiar.Value);
                await cmdCart.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return idPedidoCreado;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<PedidoEntity>> ObtenerPedidosActivosAsync()
    {
        var lista = new List<PedidoEntity>();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT id_pedido, id_mesero, id_cocinero, numero_mesa, notas, estado, fecha_hora, total
            FROM pedidos
            ORDER BY fecha_hora DESC;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(MapPedido(reader));
        }

        return lista;
    }

    public async Task<List<PedidoEntity>> ObtenerPedidosCocinaAsync()
    {
        var lista = new List<PedidoEntity>();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT id_pedido, id_mesero, id_cocinero, numero_mesa, notas, estado, fecha_hora, total
            FROM pedidos
            WHERE estado IN ('PENDIENTE', 'PREPARANDO')
            ORDER BY fecha_hora ASC;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(MapPedido(reader));
        }

        return lista;
    }

    public async Task<List<DetallePedidoEntity>> ObtenerDetallesPorPedidoAsync(int idPedido)
    {
        var lista = new List<DetallePedidoEntity>();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT dp.id_detalle_pedido,
                   dp.id_pedido,
                   dp.id_producto,
                   p.nombre AS nombre_producto,
                   dp.cantidad,
                   dp.precio_unitario,
                   dp.subtotal
            FROM detalle_pedido dp
            INNER JOIN productos p ON dp.id_producto = p.id_producto
            WHERE dp.id_pedido = @idPedido
            ORDER BY dp.id_detalle_pedido ASC;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idPedido", idPedido);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new DetallePedidoEntity
            {
                IdDetallePedido = reader.GetInt32(0),
                IdPedido = reader.GetInt32(1),
                IdProducto = reader.GetInt32(2),
                NombreProducto = reader.GetString(3),
                Cantidad = reader.GetInt32(4),
                PrecioUnitario = reader.GetDecimal(5),
                Subtotal = reader.GetDecimal(6)
            });
        }

        return lista;
    }

    public async Task<bool> ActualizarEstadoPedidoAsync(int idPedido, string nuevoEstado, int? idCocinero = null)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            UPDATE pedidos
            SET estado = @nuevoEstado,
                id_cocinero = COALESCE(@idCocinero, id_cocinero)
            WHERE id_pedido = @idPedido;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);
        cmd.Parameters.AddWithValue("@idCocinero", (object?)idCocinero ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@idPedido", idPedido);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> ActualizarPedidoCompletoAsync(
        int idPedido,
        List<DetallePedidoEntity> nuevosDetalles,
        decimal nuevoTotal)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            const string updatePedidoSql = """
                UPDATE pedidos
                SET total = @nuevoTotal,
                    estado = 'PENDIENTE'
                WHERE id_pedido = @idPedido;
                """;

            await using (var cmdPedido = new NpgsqlCommand(updatePedidoSql, conn, tx))
            {
                cmdPedido.Parameters.AddWithValue("@nuevoTotal", nuevoTotal);
                cmdPedido.Parameters.AddWithValue("@idPedido", idPedido);
                await cmdPedido.ExecuteNonQueryAsync();
            }

            const string deleteDetallesSql = "DELETE FROM detalle_pedido WHERE id_pedido = @idPedido;";
            await using (var cmdDelete = new NpgsqlCommand(deleteDetallesSql, conn, tx))
            {
                cmdDelete.Parameters.AddWithValue("@idPedido", idPedido);
                await cmdDelete.ExecuteNonQueryAsync();
            }

            const string insertDetalleSql = """
                INSERT INTO detalle_pedido (id_pedido, id_producto, cantidad, precio_unitario, subtotal)
                VALUES (@idPedido, @idProducto, @cantidad, @precioUnitario, @subtotal);
                """;

            foreach (var detalle in nuevosDetalles)
            {
                await using var cmdDet = new NpgsqlCommand(insertDetalleSql, conn, tx);
                cmdDet.Parameters.AddWithValue("@idPedido", idPedido);
                cmdDet.Parameters.AddWithValue("@idProducto", detalle.IdProducto);
                cmdDet.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                cmdDet.Parameters.AddWithValue("@precioUnitario", detalle.PrecioUnitario);
                cmdDet.Parameters.AddWithValue("@subtotal", detalle.Subtotal);
                await cmdDet.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private static PedidoEntity MapPedido(NpgsqlDataReader reader)
    {
        return new PedidoEntity
        {
            IdPedido = reader.GetInt32(0),
            IdMesero = reader.GetInt32(1),
            IdCocinero = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            NumeroMesa = reader.GetInt32(3),
            Notas = reader.IsDBNull(4) ? null : reader.GetString(4),
            Estado = reader.GetString(5),
            FechaHora = reader.GetDateTime(6),
            Total = reader.GetDecimal(7)
        };
    }
}