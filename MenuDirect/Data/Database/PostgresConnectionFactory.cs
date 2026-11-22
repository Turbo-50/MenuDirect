using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace MenuDirect.Data.Database;

public class PostgresConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(string? connectionString = null)
    {
        _connectionString = string.IsNullOrWhiteSpace(connectionString)
            ? NeonConfig.ConnectionString
            : connectionString;
    }

    public async Task<NpgsqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
