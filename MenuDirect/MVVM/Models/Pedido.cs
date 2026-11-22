using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MenuDirect.MVVM.Models;

public partial class Pedido : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int meseroId;

    [ObservableProperty]
    private int? cocineroId;

    [ObservableProperty]
    private int numeroMesa;

    [ObservableProperty]
    private string? notas;

    [ObservableProperty]
    private string estado = "PENDIENTE";

    [ObservableProperty]
    private DateTime fechaHora;

    [ObservableProperty]
    private decimal total;
}