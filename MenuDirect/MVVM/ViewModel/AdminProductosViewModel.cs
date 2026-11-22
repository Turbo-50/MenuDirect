using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MenuDirect.Data.Services;
using MenuDirect.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MenuDirect.MVVM.ViewModel;

public partial class AdminProductosViewModel : ObservableObject
{
    private readonly AdminService _adminService;
    private readonly CatalogoService _catalogoService;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string descripcion = string.Empty;

    [ObservableProperty]
    private string precioTexto = string.Empty;

    [ObservableProperty]
    private Categoria? categoriaSeleccionada;

    [ObservableProperty]
    private Subcategoria? subcategoriaSeleccionada;

    [ObservableProperty]
    private bool estaCargando;

    public ObservableCollection<Producto> Productos { get; } = new();
    public ObservableCollection<Categoria> Categorias { get; } = new();
    public ObservableCollection<Subcategoria> SubcategoriasFiltradas { get; } = new();

    public AdminProductosViewModel(
        AdminService adminService,
        CatalogoService catalogoService)
    {
        _adminService = adminService;
        _catalogoService = catalogoService;
    }

    partial void OnCategoriaSeleccionadaChanged(Categoria? value)
    {
        SubcategoriasFiltradas.Clear();
        SubcategoriaSeleccionada = null;

        if (value == null)
            return;

        foreach (var sub in _catalogoService.ObtenerSubcategoriasPorCategoria(value.Id))
            SubcategoriasFiltradas.Add(sub);
    }

    [RelayCommand]
    public async Task InicializarAsync()
    {
        if (EstaCargando)
            return;

        try
        {
            EstaCargando = true;
            await _catalogoService.CargarAsync(forzarRecarga: true);

            Categorias.Clear();
            foreach (var cat in _catalogoService.ObtenerCategorias())
                Categorias.Add(cat);

            Productos.Clear();
            foreach (var prod in await _adminService.ObtenerProductosAsync())
                Productos.Add(prod);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Error al cargar catálogo: {ex.Message}", "Aceptar");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task CrearProductoAsync()
    {
        if (CategoriaSeleccionada == null)
        {
            await Shell.Current.DisplayAlert("Validación", "Selecciona una categoría.", "Aceptar");
            return;
        }

        if (!decimal.TryParse(PrecioTexto.Trim(), out decimal precio) || precio < 0)
        {
            await Shell.Current.DisplayAlert("Validación", "Ingresa un precio válido.", "Aceptar");
            return;
        }

        try
        {
            await _adminService.CrearProductoAsync(
                Nombre,
                Descripcion,
                precio,
                CategoriaSeleccionada.Id,
                SubcategoriaSeleccionada?.Id);

            Nombre = string.Empty;
            Descripcion = string.Empty;
            PrecioTexto = string.Empty;
            CategoriaSeleccionada = null;
            SubcategoriaSeleccionada = null;

            await InicializarAsync();
            await Shell.Current.DisplayAlert("Éxito", "Producto agregado al catálogo.", "Aceptar");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private async Task ModificarPrecioAsync(Producto producto)
    {
        string? nuevoPrecioTexto = await Shell.Current.DisplayPromptAsync(
            "Modificar Precio",
            $"Nuevo precio para {producto.Nombre} (actual: ${producto.PrecioUnitario:F2}):",
            accept: "Guardar",
            cancel: "Cancelar",
            placeholder: producto.PrecioUnitario.ToString("F2"),
            keyboard: Keyboard.Numeric);

        if (nuevoPrecioTexto == null)
            return;

        if (!decimal.TryParse(nuevoPrecioTexto.Trim(), out decimal nuevoPrecio) || nuevoPrecio < 0)
        {
            await Shell.Current.DisplayAlert("Dato inválido", "Ingresa un monto numérico válido.", "Aceptar");
            return;
        }

        try
        {
            await _adminService.ModificarPrecioProductoAsync(producto.Id, nuevoPrecio);
            producto.PrecioUnitario = nuevoPrecio;
            await Shell.Current.DisplayAlert("Actualizado", "Precio modificado correctamente.", "Aceptar");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private async Task ModificarCategoriaAsync(Producto producto)
    {
        if (!Categorias.Any())
            return;

        string[] nombresCategorias = Categorias.Select(c => c.Nombre).ToArray();
        string seleccion = await Shell.Current.DisplayActionSheet(
            $"Nueva categoría para {producto.Nombre}",
            "Cancelar",
            null,
            nombresCategorias);

        if (string.IsNullOrWhiteSpace(seleccion) || seleccion == "Cancelar")
            return;

        var catElegida = Categorias.FirstOrDefault(c => c.Nombre == seleccion);
        if (catElegida == null)
            return;

        try
        {
            await _adminService.ModificarCategoriaProductoAsync(producto.Id, catElegida.Id, null);
            producto.CategoriaId = catElegida.Id;
            producto.SubcategoriaId = null;
            await Shell.Current.DisplayAlert("Actualizado", $"Categoría cambiada a {catElegida.Nombre}.", "Aceptar");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private async Task EliminarProductoAsync(Producto producto)
    {
        bool confirmar = await Shell.Current.DisplayAlert(
            "Eliminar producto",
            $"¿Deseas quitar '{producto.Nombre}' del menú?",
            "Sí, eliminar",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            await _adminService.EliminarProductoAsync(producto.Id);
            Productos.Remove(producto);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private async Task VolverAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}