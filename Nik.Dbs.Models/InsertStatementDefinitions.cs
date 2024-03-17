namespace Nik.Dbs.Models;

public class InsertStatementDefinitions
{
    public string ConnectionStringName { get; set; } = string.Empty;

    public string OutputPath { get; set; } = string.Empty;

    public string[] Tables { get; set; } = [];
}