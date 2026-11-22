using System;
using System.Collections.Generic;
using System.Text;

namespace MenuDirect.Data.Entities;

public class DetallePedidoEntity
{
    public int IdDetallePedido { get; set; }
    public int IdPedido { get; set; }
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}