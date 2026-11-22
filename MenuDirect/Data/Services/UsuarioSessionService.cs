using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.MVVM.Models;

namespace MenuDirect.Data.Services;

public class UsuarioSessionService
{
    public Usuario? UsuarioActual { get; private set; }
    public int? IdUsuario => UsuarioActual?.Id;
    public string Rol => UsuarioActual?.Rol ?? string.Empty;
    public bool EstaAutenticado => UsuarioActual != null;

    public void IniciarSesion(Usuario usuario)
    {
        UsuarioActual = usuario;
    }

    public void CerrarSesion()
    {
        UsuarioActual = null;
    }
}