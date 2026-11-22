using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;
using MenuDirect.MVVM.Models;

namespace MenuDirect.Data.Mappers;

public static class PedidoMapper
{
    public static Pedido ToModel(this PedidoEntity entity)
    {
        return new Pedido
        {
            Id = entity.IdPedido,
            MeseroId = entity.IdMesero,
            CocineroId = entity.IdCocinero,
            NumeroMesa = entity.NumeroMesa,
            Notas = entity.Notas,
            Estado = entity.Estado,
            FechaHora = entity.FechaHora,
            Total = entity.Total
        };
    }

    public static DetallePedido ToModel(this DetallePedidoEntity entity)
    {
        return new DetallePedido
        {
            Id = entity.IdDetallePedido,
            PedidoId = entity.IdPedido,
            ProductoId = entity.IdProducto,
            NombreProducto = entity.NombreProducto,
            Cantidad = entity.Cantidad,
            PrecioUnitario = entity.PrecioUnitario,
            Subtotal = entity.Subtotal
        };
    }
}