using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Interfaces;
using MenuDirect.Data.Mappers;
using MenuDirect.MVVM.Models;

namespace MenuDirect.Data.Services;

public class CatalogoService
{
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IProductoRepository _productoRepository;

    public List<Categoria> Categorias { get; private set; } = new();
    public List<Subcategoria> Subcategorias { get; private set; } = new();
    public List<Producto> Productos { get; private set; } = new();

    public CatalogoService(
        ICatalogoRepository catalogoRepository,
        IProductoRepository productoRepository)
    {
        _catalogoRepository = catalogoRepository;
        _productoRepository = productoRepository;
    }

    public async Task CargarAsync(bool forzarRecarga = false)
    {
        if (!forzarRecarga && Categorias.Any() && Productos.Any())
            return;

        var categoriasEnt = await _catalogoRepository.ObtenerCategoriasAsync();
        var subcategoriasEnt = await _catalogoRepository.ObtenerSubcategoriasAsync();
        var productosEnt = await _productoRepository.ObtenerTodosDisponiblesAsync();

        Categorias = categoriasEnt.Select(c => c.ToModel()).ToList();
        Subcategorias = subcategoriasEnt.Select(s => s.ToModel()).ToList();
        Productos = productosEnt.Select(p => p.ToModel()).ToList();
    }

    public List<Categoria> ObtenerCategorias() => Categorias;

    public List<Subcategoria> ObtenerSubcategoriasPorCategoria(int categoriaId)
    {
        return Subcategorias
            .Where(s => s.CategoriaId == categoriaId)
            .ToList();
    }

    public List<Producto> ObtenerTodosLosProductos() => Productos;

    public List<Producto> ObtenerProductosPorCategoria(int categoriaId)
    {
        return Productos
            .Where(p => p.CategoriaId == categoriaId)
            .ToList();
    }

    public List<Producto> ObtenerProductosPorSubcategoria(int subcategoriaId)
    {
        return Productos
            .Where(p => p.SubcategoriaId == subcategoriaId)
            .ToList();
    }
}