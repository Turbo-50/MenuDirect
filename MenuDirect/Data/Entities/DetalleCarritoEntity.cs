using System;
using System.Collections.Generic;
using System.Text;

namespace MenuDirect.Data.Entities;

public class DetalleCarritoEntity
{
    public int IdDetalleCarrito { get; set; }
    public int IdCarrito { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}