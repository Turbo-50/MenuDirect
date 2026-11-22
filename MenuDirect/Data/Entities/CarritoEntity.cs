using System;
using System.Collections.Generic;
using System.Text;

namespace MenuDirect.Data.Entities;

public class CarritoEntity
{
    public int IdCarrito { get; set; }
    public int IdMesero { get; set; }
    public DateTime FechaCreacion { get; set; }
    public decimal TotalGeneral { get; set; }
}