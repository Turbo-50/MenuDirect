using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;
using MenuDirect.MVVM.Models;

namespace MenuDirect.Data.Mappers;

public static class ProductoMapper
{
    public static Producto ToModel(this ProductoEntity entity)
    {
        return new Producto
        {
            Id = entity.IdProducto,
            CategoriaId = entity.IdCategoria,
            SubcategoriaId = entity.IdSubcategoria,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            PrecioUnitario = entity.Precio,
            Imagen = string.IsNullOrWhiteSpace(entity.Imagen) ? "dotnet_bot.png" : entity.Imagen,
            Disponible = entity.Disponible,
            Cantidad = 0
        };
    }

    public static ProductoEntity ToEntity(this Producto model)
    {
        return new ProductoEntity
        {
            IdProducto = model.Id,
            IdCategoria = model.CategoriaId,
            IdSubcategoria = model.SubcategoriaId,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Precio = model.PrecioUnitario,
            Imagen = model.Imagen,
            Disponible = model.Disponible
        };
    }
}