using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MenuDirect.Data.Interfaces;
using MenuDirect.Data.Mappers;
using MenuDirect.Data.Services;
using MenuDirect.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MenuDirect.MVVM.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly UsuarioSessionService _sessionService;

    [ObservableProperty]
    private string usuario = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string mensaje = string.Empty;

    [ObservableProperty]
    private bool mostrarMensaje;

    [ObservableProperty]
    private bool estaCargando;

    public LoginViewModel(
        IUsuarioRepository usuarioRepository,
        UsuarioSessionService sessionService)
    {
        _usuarioRepository = usuarioRepository;
        _sessionService = sessionService;
    }

    [RelayCommand]
    private async Task IniciarSesionAsync()
    {
        if (EstaCargando)
            return;

        MostrarMensaje = false;
        Mensaje = string.Empty;

        if (string.IsNullOrWhiteSpace(Usuario))
        {
            Mensaje = "Por favor, ingrese su usuario.";
            MostrarMensaje = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            Mensaje = "Por favor, ingrese su contraseña.";
            MostrarMensaje = true;
            return;
        }

        try
        {
            EstaCargando = true;

            var entidad = await _usuarioRepository.AutenticarAsync(Usuario, Password);
            if (entidad == null)
            {
                Mensaje = "Usuario o contraseña incorrectos.";
                MostrarMensaje = true;
                return;
            }

            var usuarioModel = entidad.ToModel();
            _sessionService.IniciarSesion(usuarioModel);

            Usuario = string.Empty;
            Password = string.Empty;

            switch (usuarioModel.Rol.ToUpperInvariant())
            {
                case "ADMINISTRADOR":
                    await Shell.Current.GoToAsync("//admin");
                    break;
                case "MESERO":
                    await Shell.Current.GoToAsync("//mesero");
                    break;
                case "COCINA":
                    await Shell.Current.GoToAsync("//cocina");
                    break;
                default:
                    Mensaje = $"Rol no reconocido: {usuarioModel.Rol}";
                    MostrarMensaje = true;
                    break;
            }
        }
        catch (Exception ex)
        {
            Mensaje = $"Error de conexión con la base de datos: {ex.Message}";
            MostrarMensaje = true;
        }
        finally
        {
            EstaCargando = false;
        }
    }
}