namespace Nik.Dbs.Models;

public class ScaffoldDefinition
{
    public string ConnectionStringName { get; set; } = string.Empty;

    public string OutputPath { get; set; } = string.Empty;

    public ScaffoldDefinitionTable[] Tables { get; set; } = [];
}

public class ScaffoldDefinitionTable
{
    public string TableName { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;
}