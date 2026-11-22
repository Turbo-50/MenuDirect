using System;
using System.Collections.Generic;
using System.Text;

namespace MenuDirect.Data.Entities;

public class UsuarioEntity
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
