using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;

namespace MenuDirect.Data.Interfaces;

public interface ICarritoRepository
{
    Task<CarritoEntity> ObtenerOCrearCarritoPorMeseroAsync(int idMesero);
    Task<List<DetalleCarritoEntity>> ObtenerDetallesCarritoAsync(int idCarrito);
    Task GuardarOActualizarDetalleAsync(int idCarrito, int idProducto, int cantidad, decimal precioUnitario);
    Task EliminarDetalleProductoAsync(int idCarrito, int idProducto);
    Task VaciarCarritoAsync(int idCarrito);
}