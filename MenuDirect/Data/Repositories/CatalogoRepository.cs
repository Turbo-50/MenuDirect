using System;
using System.Collections.Generic;
using System.Text;
using MenuDirect.Data.Database;
using MenuDirect.Data.Entities;
using MenuDirect.Data.Interfaces;
using Npgsql;

namespace MenuDirect.Data.Repositories;

public class CatalogoRepository : ICatalogoRepository
{
    private readonly PostgresConnectionFactory _connectionFactory;

    public CatalogoRepository(PostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<CategoriaEntity>> ObtenerCategoriasAsync()
    {
        var lista = new List<CategoriaEntity>();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT id_categoria, nombre, descripcion
            FROM categorias
            ORDER BY id_categoria ASC;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new CategoriaEntity
            {
                IdCategoria = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2)
            });
        }

        return lista;
    }

    public async Task<List<SubcategoriaEntity>> ObtenerSubcategoriasAsync()
    {
        var lista = new List<SubcategoriaEntity>();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT id_subcategoria, id_categoria, nombre
            FROM subcategorias
            ORDER BY id_subcategoria ASC;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new SubcategoriaEntity
            {
                IdSubcategoria = reader.GetInt32(0),
                IdCategoria = reader.GetInt32(1),
                Nombre = reader.GetString(2)
            });
        }

        return lista;
    }

    public async Task<List<SubcategoriaEntity>> ObtenerSubcategoriasPorCategoriaAsync(int idCategoria)
    {
        var lista = new List<SubcategoriaEntity>();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = """
            SELECT id_subcategoria, id_categoria, nombre
            FROM subcategorias
            WHERE id_categoria = @idCategoria
            ORDER BY id_subcategoria ASC;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idCategoria", idCategoria);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new SubcategoriaEntity
            {
                IdSubcategoria = reader.GetInt32(0),
                IdCategoria = reader.GetInt32(1),
                Nombre = reader.GetString(2)
            });
        }

        return lista;
    }
}