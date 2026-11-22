using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;
using MenuDirect.MVVM.Models;

namespace MenuDirect.Data.Mappers;

public static class UsuarioMapper
{
    public static Usuario ToModel(this UsuarioEntity entity)
    {
        return new Usuario
        {
            Id = entity.IdUsuario,
            Nombre = entity.Nombre,
            NombreUsuario = entity.NombreUsuario,
            Rol = entity.Rol,
            Activo = entity.Activo
        };
    }

    public static UsuarioEntity ToEntity(this Usuario model, string contrasena = "")
    {
        return new UsuarioEntity
        {
            IdUsuario = model.Id,
            Nombre = model.Nombre,
            NombreUsuario = model.NombreUsuario,
            Contrasena = contrasena,
            Rol = model.Rol,
            Activo = model.Activo
        };
    }
}