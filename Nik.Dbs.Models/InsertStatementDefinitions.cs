namespace Nik.Dbs.Models;

public class InsertStatementDefinitions
{
    public string ConnectionStringName { get; set; } = string.Empty;

    public string OutputPath { get; set; } = string.Empty;

    public InsertStatementTable[] Tables { get; set; } = [];
}

public class InsertStatementTable
{
    public string TableName { get; set; } = string.Empty;

    public string FullTableName { get; set; } = string.Empty;
}