using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MenuDirect.Data.Services;
using MenuDirect.MVVM.Models;

namespace MenuDirect.MVVM.ViewModel;

public class ComandaCocinaItem
{
    public int IdPedido { get; set; }
    public int NumeroMesa { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
    public bool TieneNotas { get; set; }
    public DateTime FechaHora { get; set; }
    public string ResumenProductos { get; set; } = string.Empty;
    public Color ColorEstado => Estado == "PENDIENTE" ? Colors.DarkOrange : Colors.CornflowerBlue;
}

public partial class CocinaViewModel : ObservableObject
{
    private readonly PedidoService _pedidoService;
    private readonly UsuarioSessionService _sessionService;

    [ObservableProperty]
    private bool estaCargando;

    public ObservableCollection<ComandaCocinaItem> Comandas { get; } = new();

    public CocinaViewModel(
        PedidoService pedidoService,
        UsuarioSessionService sessionService)
    {
        _pedidoService = pedidoService;
        _sessionService = sessionService;
    }

    [RelayCommand]
    public async Task CargarComandasAsync()
    {
        if (EstaCargando)
            return;

        try
        {
            EstaCargando = true;
            Comandas.Clear();

            var pedidos = await _pedidoService.ObtenerPedidosCocinaAsync();
            foreach (var pedido in pedidos)
            {
                var detalles = await _pedidoService.ObtenerDetallesPedidoAsync(pedido.Id);
                string resumen = string.Join("\n", detalles.Select(d => $"• {d.Cantidad}x {d.NombreProducto}"));

                Comandas.Add(new ComandaCocinaItem
                {
                    IdPedido = pedido.Id,
                    NumeroMesa = pedido.NumeroMesa,
                    Estado = pedido.Estado,
                    Notas = string.IsNullOrWhiteSpace(pedido.Notas) ? string.Empty : $"Nota: {pedido.Notas}",
                    TieneNotas = !string.IsNullOrWhiteSpace(pedido.Notas),
                    FechaHora = pedido.FechaHora,
                    ResumenProductos = resumen
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Error al cargar comandas: {ex.Message}", "Aceptar");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task MarcarPreparandoAsync(ComandaCocinaItem comanda)
    {
        if (comanda.Estado == "PREPARANDO")
            return;

        try
        {
            await _pedidoService.CambiarEstadoPedidoAsync(comanda.IdPedido, "PREPARANDO");
            await CargarComandasAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private async Task MarcarListoAsync(ComandaCocinaItem comanda)
    {
        try
        {
            await _pedidoService.CambiarEstadoPedidoAsync(comanda.IdPedido, "LISTO");
            Comandas.Remove(comanda);
            await Shell.Current.DisplayAlert("Comanda Lista", $"Pedido #{comanda.IdPedido} (Mesa {comanda.NumeroMesa}) marcado como LISTO.", "Aceptar");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private async Task CerrarSesionAsync()
    {
        _sessionService.CerrarSesion();
        await Shell.Current.GoToAsync("//login");
    }
}