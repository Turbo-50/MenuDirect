using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MenuDirect.Data.Services;
using MenuDirect.MVVM.Models;

namespace MenuDirect.MVVM.ViewModel;

public partial class PedidoListViewModel : ObservableObject
{
    private readonly PedidoService _pedidoService;
    private readonly CarritoService _carritoService;

    [ObservableProperty]
    private bool estaCargando;

    public ObservableCollection<Pedido> Pedidos { get; } = new();

    public PedidoListViewModel(
        PedidoService pedidoService,
        CarritoService carritoService)
    {
        _pedidoService = pedidoService;
        _carritoService = carritoService;
    }

    [RelayCommand]
    public async Task CargarPedidosAsync()
    {
        if (EstaCargando)
            return;

        try
        {
            EstaCargando = true;
            Pedidos.Clear();

            var lista = await _pedidoService.ObtenerPedidosActivosAsync();
            foreach (var pedido in lista)
                Pedidos.Add(pedido);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar los pedidos: {ex.Message}", "Aceptar");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task VerDetalleAsync(Pedido pedido)
    {
        try
        {
            var detalles = await _pedidoService.ObtenerDetallesPedidoAsync(pedido.Id);
            var lineas = detalles.Select(d => $"• {d.Cantidad}x {d.NombreProducto} (${d.Subtotal:F2})");
            string resumen = string.Join("\n", lineas);
            string notaTexto = string.IsNullOrWhiteSpace(pedido.Notas) ? "Sin notas" : pedido.Notas;

            await Shell.Current.DisplayAlert(
                $"Pedido #{pedido.Id} - Mesa {pedido.NumeroMesa}",
                $"Estado: {pedido.Estado}\nNotas: {notaTexto}\n\nProductos:\n{resumen}\n\nTotal: ${pedido.Total:F2}",
                "Cerrar");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private async Task ModificarPedidoAsync(Pedido pedido)
    {
        try
        {
            await _carritoService.CargarPedidoParaEdicionAsync(pedido.Id);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar el pedido: {ex.Message}", "Aceptar");
        }
    }

    [RelayCommand]
    private async Task VolverAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}