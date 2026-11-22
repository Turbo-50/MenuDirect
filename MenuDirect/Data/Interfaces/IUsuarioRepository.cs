using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;

namespace MenuDirect.Data.Interfaces;

public interface IUsuarioRepository
{
    Task<UsuarioEntity?> AutenticarAsync(string nombreUsuario, string contrasena);
    Task<List<UsuarioEntity>> ObtenerEmpleadosActivosAsync();
    Task<int> CrearEmpleadoAsync(UsuarioEntity empleado);
    Task<bool> EliminarEmpleadoAsync(int idUsuario);
}