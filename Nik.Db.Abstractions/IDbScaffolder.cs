namespace Nik.Db.Abstractions;

public interface IDbScaffolder
{
    Task ScaffoldAsync(string connectionStringName, string tableName, string outputPath);
}