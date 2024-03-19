namespace Nik.Dbs;

public sealed class SchemaTableGenerater : ISchemaTableGenerater
{
    public async Task<DataTable> GenerateAsync(string connectionString, string tableName)
    {
        using SqlConnection connection = new(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT 
                c.name AS COLUMN_NAME,
                t.name AS DATA_TYPE,
                c.max_length AS CHARACTER_MAXIMUM_LENGTH,
                c.precision AS NUMERIC_PRECISION,
                c.scale AS NUMERIC_SCALE,
                c.IS_IDENTITY,
                c.IS_NULLABLE,
                c.column_id AS ORDINAL_POSITION
            FROM sys.columns c
            JOIN sys.types t ON c.system_type_id = t.system_type_id
            JOIN sys.tables tb ON c.object_id = tb.object_id
            WHERE tb.name = '{tableName}' AND t.name != 'sysname';            
            """;
        command.Parameters.AddWithValue("@tableName", tableName);

        DataTable schemaTable = new DataTable();
        using SqlDataAdapter adapter = new(command);
        adapter.Fill(schemaTable);

        await connection.CloseAsync();

        return schemaTable;
    }
}