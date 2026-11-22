using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MenuDirect.MVVM.Models;

public partial class DetallePedido : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int pedidoId;

    [ObservableProperty]
    private int productoId;

    [ObservableProperty]
    private string nombreProducto = string.Empty;

    [ObservableProperty]
    private int cantidad;

    [ObservableProperty]
    private decimal precioUnitario;

    [ObservableProperty]
    private decimal subtotal;
}