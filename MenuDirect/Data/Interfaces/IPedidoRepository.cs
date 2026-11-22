using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;

namespace MenuDirect.Data.Interfaces;

public interface IPedidoRepository
{
    Task<int> CrearPedidoCompletoAsync(PedidoEntity pedido, List<DetallePedidoEntity> detalles, int? idCarritoALimpiar = null);
    Task<List<PedidoEntity>> ObtenerPedidosActivosAsync();
    Task<List<PedidoEntity>> ObtenerPedidosCocinaAsync();
    Task<List<DetallePedidoEntity>> ObtenerDetallesPorPedidoAsync(int idPedido);
    Task<bool> ActualizarEstadoPedidoAsync(int idPedido, string nuevoEstado, int? idCocinero = null);
    Task<bool> ActualizarPedidoCompletoAsync(int idPedido, List<DetallePedidoEntity> nuevosDetalles, decimal nuevoTotal);
}