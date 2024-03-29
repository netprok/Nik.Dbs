namespace Nik.Dbs.Models;

public class ScaffoldDefinition
{
    public string ConnectionStringName { get; set; } = string.Empty;

    public string OutputPath { get; set; } = string.Empty;

    public Table[] Tables { get; set; } = [];

    public bool AddUsings { get; set; } = true;

    public string? Namespace { get; set; }

    public class Table : TableBase
    {
        public string ClassName { get; set; } = string.Empty;
    }
}