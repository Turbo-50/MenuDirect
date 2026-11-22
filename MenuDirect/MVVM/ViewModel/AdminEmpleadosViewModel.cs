using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MenuDirect.Data.Services;
using MenuDirect.MVVM.Models;

namespace MenuDirect.MVVM.ViewModel;

public partial class AdminEmpleadosViewModel : ObservableObject
{
    private readonly AdminService _adminService;
    private readonly UsuarioSessionService _sessionService;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string nombreUsuario = string.Empty;

    [ObservableProperty]
    private string contrasena = string.Empty;

    [ObservableProperty]
    private string rolSeleccionado = "MESERO";

    [ObservableProperty]
    private bool estaCargando;

    public List<string> RolesDisponibles { get; } = new() { "MESERO", "COCINA", "ADMINISTRADOR" };
    public ObservableCollection<Usuario> Empleados { get; } = new();

    public AdminEmpleadosViewModel(
        AdminService adminService,
        UsuarioSessionService sessionService)
    {
        _adminService = adminService;
        _sessionService = sessionService;
    }

    [RelayCommand]
    public async Task CargarEmpleadosAsync()
    {
        if (EstaCargando)
            return;

        try
        {
            EstaCargando = true;
            Empleados.Clear();

            var lista = await _adminService.ObtenerEmpleadosAsync();
            foreach (var emp in lista)
                Empleados.Add(emp);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Error al cargar empleados: {ex.Message}", "Aceptar");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task CrearEmpleadoAsync()
    {
        try
        {
            await _adminService.CrearEmpleadoAsync(Nombre, NombreUsuario, Contrasena, RolSeleccionado);

            Nombre = string.Empty;
            NombreUsuario = string.Empty;
            Contrasena = string.Empty;
            RolSeleccionado = "MESERO";

            await CargarEmpleadosAsync();
            await Shell.Current.DisplayAlert("Éxito", "Empleado registrado correctamente.", "Aceptar");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    [RelayCommand]
    private async Task EliminarEmpleadoAsync(Usuario empleado)
    {
        if (_sessionService.IdUsuario == empleado.Id)
        {
            await Shell.Current.DisplayAlert("Acción no permitida", "No puedes eliminar tu propio usuario en sesión.", "Aceptar");
            return;
        }

        bool confirmar = await Shell.Current.DisplayAlert(
            "Eliminar empleado",
            $"¿Deseas dar de baja a {empleado.Nombre} ({empleado.Rol})?",
            "Sí, eliminar",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            await _adminService.EliminarEmpleadoAsync(empleado.Id);
            Empleados.Remove(empleado);
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