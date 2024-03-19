namespace Nik.Dbs.Abstractions;

public interface ISchemaTableGenerater
{
    Task<DataTable> GenerateAsync(string connectionString, string tableName);
}