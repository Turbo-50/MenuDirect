using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;
using MenuDirect.MVVM.Models;

namespace MenuDirect.Data.Mappers;

public static class CategoriaMapper
{
    public static Categoria ToModel(this CategoriaEntity entity)
    {
        return new Categoria
        {
            Id = entity.IdCategoria,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            IsSelected = false
        };
    }

    public static Subcategoria ToModel(this SubcategoriaEntity entity)
    {
        return new Subcategoria
        {
            Id = entity.IdSubcategoria,
            CategoriaId = entity.IdCategoria,
            Nombre = entity.Nombre,
            IsSelected = false
        };
    }
}