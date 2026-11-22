using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MenuDirect.Data.Services;
using MenuDirect.MVVM.Models;

namespace MenuDirect.MVVM.ViewModel;

public partial class CarritoViewModel : ObservableObject
{
    private readonly CarritoService _carritoService;
    private readonly PedidoService _pedidoService;

    [ObservableProperty]
    private bool estaProcesando;

    public CarritoViewModel(
        CarritoService carritoService,
        PedidoService pedidoService)
    {
        _carritoService = carritoService;
        _pedidoService = pedidoService;

        _carritoService.PropertyChanged += (_, e) =>
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == nameof(CarritoService.EditandoPedido))
                OnPropertyChanged(nameof(TextoBotonOrdenar));
        };
    }

    public ObservableCollection<Producto> ProductosSeleccionados => _carritoService.ProductosSeleccionados;
    public decimal TotalPedido => _carritoService.TotalPedido;
    public int CantidadProductosCarrito => _carritoService.CantidadProductosCarrito;
    public bool EditandoPedido => _carritoService.EditandoPedido;
    public string TextoBotonOrdenar => _carritoService.EditandoPedido ? "Actualizar" : "Ordenar";

    [RelayCommand]
    private async Task OrdenarAsync()
    {
        if (EstaProcesando)
            return;

        if (!ProductosSeleccionados.Any())
        {
            await Shell.Current.DisplayAlert("Carrito vacío", "Agrega productos antes de confirmar la orden.", "Aceptar");
            return;
        }

        try
        {
            EstaProcesando = true;

            if (_carritoService.EditandoPedido)
            {
                int idPedido = _carritoService.PedidoEnEdicion!.Value;
                await _pedidoService.ActualizarPedidoExistenteAsync(idPedido);
                await Shell.Current.DisplayAlert("Pedido actualizado", $"El pedido #{idPedido} se actualizó y envió a cocina.", "Aceptar");
            }
            else
            {
                string? mesaTexto = await Shell.Current.DisplayPromptAsync(
                    "Confirmar Orden",
                    "Ingrese el número de mesa:",
                    accept: "Continuar",
                    cancel: "Cancelar",
                    placeholder: "Ej. 5",
                    keyboard: Keyboard.Numeric);

                if (mesaTexto == null)
                    return;

                if (!int.TryParse(mesaTexto.Trim(), out int numeroMesa) || numeroMesa <= 0)
                {
                    await Shell.Current.DisplayAlert("Dato inválido", "Ingrese un número de mesa válido mayor a 0.", "Aceptar");
                    return;
                }

                string? notas = await Shell.Current.DisplayPromptAsync(
                    $"Mesa #{numeroMesa}",
                    "Notas o especificaciones para cocina (opcional):",
                    accept: "Enviar a Cocina",
                    cancel: "Omitir notas",
                    placeholder: "Ej. Sin cebolla, salsa aparte");

                int idPedidoCreado = await _pedidoService.ConfirmarOrdenDesdeCarritoAsync(numeroMesa, notas);
                await Shell.Current.DisplayAlert(
                    "Orden enviada",
                    $"Pedido #{idPedidoCreado} (Mesa {numeroMesa}) enviado a cocina.",
                    "Aceptar");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo procesar el pedido: {ex.Message}", "Aceptar");
        }
        finally
        {
            EstaProcesando = false;
        }
    }

    [RelayCommand]
    private async Task PedidosAsync()
    {
        await Shell.Current.GoToAsync("PedidoListView");
    }

    [RelayCommand]
    private async Task CancelarPedidoAsync()
    {
        if (!ProductosSeleccionados.Any() && !EditandoPedido)
        {
            await Shell.Current.DisplayAlert("Cancelar", "No hay productos en el carrito.", "Aceptar");
            return;
        }

        string mensaje = EditandoPedido
            ? "¿Deseas salir de la edición del pedido actual sin guardar los cambios?"
            : "¿Seguro que deseas vaciar todos los productos del carrito?";

        bool confirmar = await Shell.Current.DisplayAlert("Confirmar cancelación", mensaje, "Sí", "No");
        if (!confirmar)
            return;

        await _carritoService.CancelarPedidoAsync();
    }
}