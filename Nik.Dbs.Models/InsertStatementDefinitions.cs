namespace Nik.Dbs.Models;

public class InsertStatementDefinitions
{
    public string ConnectionStringName { get; set; } = string.Empty;

    public string OutputPath { get; set; } = string.Empty;

    public Table[] Tables { get; set; } = [];

    public class Table : TableBase
    {
        public string FullTableName { get; set; } = string.Empty;
    }
}