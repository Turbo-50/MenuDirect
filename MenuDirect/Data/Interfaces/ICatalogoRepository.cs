using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;

namespace MenuDirect.Data.Interfaces;

public interface ICatalogoRepository
{
    Task<List<CategoriaEntity>> ObtenerCategoriasAsync();
    Task<List<SubcategoriaEntity>> ObtenerSubcategoriasAsync();
    Task<List<SubcategoriaEntity>> ObtenerSubcategoriasPorCategoriaAsync(int idCategoria);
}