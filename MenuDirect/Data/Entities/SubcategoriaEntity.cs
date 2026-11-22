using System;
using System.Collections.Generic;
using System.Text;

namespace MenuDirect.Data.Entities;

public class SubcategoriaEntity
{
    public int IdSubcategoria { get; set; }
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
}