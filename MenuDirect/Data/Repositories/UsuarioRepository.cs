using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Database;
using MenuDirect.Data.Entities;
using MenuDirect.Data.Interfaces;
using Npgsql;

namespace MenuDirect.Data.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly PostgresConnectionFactory _connectionFactory;

    public UsuarioRepository(PostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UsuarioEntity?> AutenticarAsync(string nombreUsuario, string contrasena)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT id_usuario, nombre, nombre_usuario, contrasena, rol, activo
            FROM usuarios
            WHERE nombre_usuario = @usuario
              AND contrasena = @contrasena
              AND activo = TRUE
            LIMIT 1;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@usuario", nombreUsuario.Trim());
        cmd.Parameters.AddWithValue("@contrasena", contrasena);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new UsuarioEntity
            {
                IdUsuario = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                NombreUsuario = reader.GetString(2),
                Contrasena = reader.GetString(3),
                Rol = reader.GetString(4),
                Activo = reader.GetBoolean(5)
            };
        }

        return null;
    }

    public async Task<List<UsuarioEntity>> ObtenerEmpleadosActivosAsync()
    {
        var lista = new List<UsuarioEntity>();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT id_usuario, nombre, nombre_usuario, contrasena, rol, activo
            FROM usuarios
            WHERE activo = TRUE
            ORDER BY id_usuario ASC;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new UsuarioEntity
            {
                IdUsuario = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                NombreUsuario = reader.GetString(2),
                Contrasena = reader.GetString(3),
                Rol = reader.GetString(4),
                Activo = reader.GetBoolean(5)
            });
        }

        return lista;
    }

    public async Task<int> CrearEmpleadoAsync(UsuarioEntity empleado)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            INSERT INTO usuarios (nombre, nombre_usuario, contrasena, rol, activo)
            VALUES (@nombre, @nombreUsuario, @contrasena, @rol, TRUE)
            RETURNING id_usuario;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nombre", empleado.Nombre.Trim());
        cmd.Parameters.AddWithValue("@nombreUsuario", empleado.NombreUsuario.Trim());
        cmd.Parameters.AddWithValue("@contrasena", empleado.Contrasena);
        cmd.Parameters.AddWithValue("@rol", empleado.Rol);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> EliminarEmpleadoAsync(int idUsuario)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            UPDATE usuarios
            SET activo = FALSE
            WHERE id_usuario = @idUsuario;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}