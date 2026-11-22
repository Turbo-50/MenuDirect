using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;
using MenuDirect.Data.Interfaces;
using MenuDirect.Data.Mappers;
using MenuDirect.MVVM.Models;

namespace MenuDirect.Data.Services;

public class AdminService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly CatalogoService _catalogoService;

    public AdminService(
        IUsuarioRepository usuarioRepository,
        IProductoRepository productoRepository,
        CatalogoService catalogoService)
    {
        _usuarioRepository = usuarioRepository;
        _productoRepository = productoRepository;
        _catalogoService = catalogoService;
    }

    #region Gestión de Empleados
    public async Task<List<Usuario>> ObtenerEmpleadosAsync()
    {
        var lista = await _usuarioRepository.ObtenerEmpleadosActivosAsync();
        return lista.Select(u => u.ToModel()).ToList();
    }

    public async Task<int> CrearEmpleadoAsync(string nombre, string nombreUsuario, string contrasena, string rol)
    {
        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(nombreUsuario) ||
            string.IsNullOrWhiteSpace(contrasena) ||
            string.IsNullOrWhiteSpace(rol))
        {
            throw new ArgumentException("Todos los campos del empleado son obligatorios.");
        }

        var entidad = new UsuarioEntity
        {
            Nombre = nombre.Trim(),
            NombreUsuario = nombreUsuario.Trim(),
            Contrasena = contrasena,
            Rol = rol.ToUpperInvariant(),
            Activo = true
        };

        return await _usuarioRepository.CrearEmpleadoAsync(entidad);
    }

    public async Task<bool> EliminarEmpleadoAsync(int idUsuario)
    {
        return await _usuarioRepository.EliminarEmpleadoAsync(idUsuario);
    }
    #endregion

    #region Gestión de Productos
    public async Task<List<Producto>> ObtenerProductosAsync()
    {
        var lista = await _productoRepository.ObtenerTodosDisponiblesAsync();
        return lista.Select(p => p.ToModel()).ToList();
    }

    public async Task<int> CrearProductoAsync(
        string nombre,
        string? descripcion,
        decimal precio,
        int categoriaId,
        int? subcategoriaId,
        string? imagen = "dotnet_bot.png")
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del producto es obligatorio.");

        if (precio < 0)
            throw new ArgumentException("El precio no puede ser negativo.");

        if (categoriaId <= 0)
            throw new ArgumentException("Debe seleccionar una categoría válida.");

        var entidad = new ProductoEntity
        {
            Nombre = nombre.Trim(),
            Descripcion = descripcion,
            Precio = precio,
            IdCategoria = categoriaId,
            IdSubcategoria = subcategoriaId,
            Imagen = string.IsNullOrWhiteSpace(imagen) ? "dotnet_bot.png" : imagen,
            Disponible = true
        };

        int id = await _productoRepository.CrearProductoAsync(entidad);
        await _catalogoService.CargarAsync(forzarRecarga: true);
        return id;
    }

    public async Task<bool> EliminarProductoAsync(int idProducto)
    {
        bool ok = await _productoRepository.EliminarProductoAsync(idProducto);
        if (ok)
            await _catalogoService.CargarAsync(forzarRecarga: true);
        return ok;
    }

    public async Task<bool> ModificarPrecioProductoAsync(int idProducto, decimal nuevoPrecio)
    {
        if (nuevoPrecio < 0)
            throw new ArgumentException("El precio no puede ser negativo.");

        bool ok = await _productoRepository.ModificarPrecioAsync(idProducto, nuevoPrecio);
        if (ok)
            await _catalogoService.CargarAsync(forzarRecarga: true);
        return ok;
    }

    public async Task<bool> ModificarCategoriaProductoAsync(int idProducto, int categoriaId, int? subcategoriaId)
    {
        if (categoriaId <= 0)
            throw new ArgumentException("Debe seleccionar una categoría válida.");

        bool ok = await _productoRepository.ModificarCategoriaAsync(idProducto, categoriaId, subcategoriaId);
        if (ok)
            await _catalogoService.CargarAsync(forzarRecarga: true);
        return ok;
    }
    #endregion
}