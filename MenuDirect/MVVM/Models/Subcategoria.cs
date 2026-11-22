using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MenuDirect.MVVM.Models;

public partial class Subcategoria : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int categoriaId;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private bool isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(TextColor));
    }

    public Color BackgroundColor => IsSelected ? Colors.CornflowerBlue : Colors.White;
    public Color TextColor => IsSelected ? Colors.White : Colors.Black;
}