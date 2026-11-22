using System;
using System.Collections.Generic;
using System.Text;

namespace MenuDirect.Data.Entities;

public class ProductoEntity
{
    public int IdProducto { get; set; }
    public int IdCategoria { get; set; }
    public int? IdSubcategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string? Imagen { get; set; } = "dotnet_bot.png";
    public bool Disponible { get; set; } = true;
}