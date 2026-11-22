using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MenuDirect.MVVM.Models;

public partial class Usuario : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string nombreUsuario = string.Empty;

    [ObservableProperty]
    private string rol = string.Empty;

    [ObservableProperty]
    private bool activo = true;
}