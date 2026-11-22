using System;
using System.Collections.Generic;
using System.Text;

namespace MenuDirect.Data.Entities;

public class CategoriaEntity
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}