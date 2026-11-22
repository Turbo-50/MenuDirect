using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;

namespace MenuDirect.Data.Interfaces;

public interface IProductoRepository
{
    Task<List<ProductoEntity>> ObtenerTodosDisponiblesAsync();
    Task<int> CrearProductoAsync(ProductoEntity producto);
    Task<bool> EliminarProductoAsync(int idProducto);
    Task<bool> ModificarPrecioAsync(int idProducto, decimal nuevoPrecio);
    Task<bool> ModificarCategoriaAsync(int idProducto, int idCategoria, int? idSubcategoria);
}