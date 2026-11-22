using System;
using System.Collections.Generic;
using System.Text;

namespace MenuDirect.Data.Entities;

public class PedidoEntity
{
    public int IdPedido { get; set; }
    public int IdMesero { get; set; }
    public int? IdCocinero { get; set; }
    public int NumeroMesa { get; set; }
    public string? Notas { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public DateTime FechaHora { get; set; }
    public decimal Total { get; set; }
}