using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Entities;
using MenuDirect.Data.Interfaces;
using MenuDirect.Data.Mappers;
using MenuDirect.MVVM.Models;

namespace MenuDirect.Data.Services;

public class PedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly CarritoService _carritoService;
    private readonly UsuarioSessionService _sessionService;

    public PedidoService(
        IPedidoRepository pedidoRepository,
        CarritoService carritoService,
        UsuarioSessionService sessionService)
    {
        _pedidoRepository = pedidoRepository;
        _carritoService = carritoService;
        _sessionService = sessionService;
    }

    public async Task<int> ConfirmarOrdenDesdeCarritoAsync(int numeroMesa, string? notas)
    {
        var items = _carritoService.ProductosSeleccionados.Where(p => p.Cantidad > 0).ToList();
        if (!items.Any())
            throw new InvalidOperationException("No hay productos en el carrito para ordenar.");

        if (numeroMesa <= 0)
            throw new ArgumentException("El número de mesa debe ser mayor a cero.");

        int idMesero = _sessionService.IdUsuario
            ?? throw new InvalidOperationException("No hay una sesión de mesero activa.");

        var pedidoEntity = new PedidoEntity
        {
            IdMesero = idMesero,
            NumeroMesa = numeroMesa,
            Notas = notas,
            Estado = "PENDIENTE",
            Total = items.Sum(i => i.Subtotal)
        };

        var detallesEntity = items.Select(p => new DetallePedidoEntity
        {
            IdProducto = p.Id,
            NombreProducto = p.Nombre,
            Cantidad = p.Cantidad,
            PrecioUnitario = p.PrecioUnitario,
            Subtotal = p.Subtotal
        }).ToList();

        int idPedidoCreado = await _pedidoRepository.CrearPedidoCompletoAsync(
            pedidoEntity,
            detallesEntity,
            _carritoService.IdCarritoActivo);

        _carritoService.LimpiarDespuesDeOrdenar();
        return idPedidoCreado;
    }

    public async Task ActualizarPedidoExistenteAsync(int idPedido)
    {
        var items = _carritoService.ProductosSeleccionados.Where(p => p.Cantidad > 0).ToList();
        if (!items.Any())
            throw new InvalidOperationException("El pedido debe contener al menos un producto.");

        var detallesEntity = items.Select(p => new DetallePedidoEntity
        {
            IdPedido = idPedido,
            IdProducto = p.Id,
            NombreProducto = p.Nombre,
            Cantidad = p.Cantidad,
            PrecioUnitario = p.PrecioUnitario,
            Subtotal = p.Subtotal
        }).ToList();

        decimal nuevoTotal = items.Sum(i => i.Subtotal);

        await _pedidoRepository.ActualizarPedidoCompletoAsync(idPedido, detallesEntity, nuevoTotal);
        _carritoService.LimpiarDespuesDeOrdenar();
    }

    public async Task<List<Pedido>> ObtenerPedidosActivosAsync()
    {
        var entidades = await _pedidoRepository.ObtenerPedidosActivosAsync();
        return entidades.Select(p => p.ToModel()).ToList();
    }

    public async Task<List<Pedido>> ObtenerPedidosCocinaAsync()
    {
        var entidades = await _pedidoRepository.ObtenerPedidosCocinaAsync();
        return entidades.Select(p => p.ToModel()).ToList();
    }

    public async Task<List<DetallePedido>> ObtenerDetallesPedidoAsync(int idPedido)
    {
        var entidades = await _pedidoRepository.ObtenerDetallesPorPedidoAsync(idPedido);
        return entidades.Select(d => d.ToModel()).ToList();
    }

    public async Task<bool> CambiarEstadoPedidoAsync(int idPedido, string nuevoEstado)
    {
        return await _pedidoRepository.ActualizarEstadoPedidoAsync(
            idPedido,
            nuevoEstado,
            _sessionService.IdUsuario);
    }
}