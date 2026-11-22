using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MenuDirect.MVVM.Models;

public partial class Producto : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int categoriaId;

    [ObservableProperty]
    private int? subcategoriaId;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string? descripcion;

    [ObservableProperty]
    private decimal precioUnitario;

    [ObservableProperty]
    private string imagen = "dotnet_bot.png";

    [ObservableProperty]
    private bool disponible = true;

    [ObservableProperty]
    private int cantidad;

    public bool MostrarCarrito => Cantidad == 0;
    public bool MostrarCantidad => Cantidad > 0;
    public decimal Subtotal => PrecioUnitario * Cantidad;

    partial void OnCantidadChanged(int value)
    {
        OnPropertyChanged(nameof(MostrarCarrito));
        OnPropertyChanged(nameof(MostrarCantidad));
        OnPropertyChanged(nameof(Subtotal));
    }

    partial void OnPrecioUnitarioChanged(decimal value)
    {
        OnPropertyChanged(nameof(Subtotal));
    }

    public void Refrescar()
    {
        OnPropertyChanged(nameof(Cantidad));
        OnPropertyChanged(nameof(MostrarCarrito));
        OnPropertyChanged(nameof(MostrarCantidad));
        OnPropertyChanged(nameof(Subtotal));
    }
}