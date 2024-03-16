namespace Nke.Dbs.Console.Scaffold;

public class JtlScaffoldPropertyNameGenerater : IScaffoldPropertyNameGenerater
{
    public string Generate(string columnName)
    {
        string propertyName = columnName;
        if (columnName.StartsWith('k'))
        {
            propertyName = columnName[1..] + "Id";
        }
        else if (columnName.Length > 4 && columnName.StartsWith('t') && columnName.Contains("_k"))
        {
            int index = columnName.LastIndexOf("_k");
            propertyName = columnName[1..index] + "Id";
        }
        else if (columnName.StartsWith('d'))
        {
            propertyName = columnName[1..] + "Time";
        }
        else if (columnName.StartsWith('c') || columnName.StartsWith('f') || columnName.StartsWith('n'))
        {
            propertyName = columnName[1..];
        }

        return propertyName;
    }
}