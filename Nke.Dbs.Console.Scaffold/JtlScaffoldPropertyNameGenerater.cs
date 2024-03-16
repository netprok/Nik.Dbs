namespace Nke.Dbs.Console.Scaffold;

public class JtlScaffoldPropertyNameGenerater : IScaffoldPropertyNameGenerater
{
    private const string UnknownColumnFormatError = "Unknown column format";

    public string Generate(string columnName, string tableName)
    {
        if (!char.IsUpper(columnName[1]))
        {
            throw new Exception(UnknownColumnFormatError);
        }

        // key
        if (columnName.StartsWith('k'))
        {
            // primary key
            if (tableName[1..] == columnName[1..])
            {
                return "Id";
            }
            // foreign key
            return columnName[1..] + "Id";
        }
        // foreign key
        if (columnName.Length > 4 && columnName.StartsWith('t') && columnName.Contains("_k"))
        {
            int index = columnName.LastIndexOf("_k");
            return columnName[1..index] + "Id";
        }
        // date time
        if (columnName.StartsWith('d'))
        {
            return columnName[1..] + "Time";
        }
        // other types
        if (columnName.StartsWith('c') || columnName.StartsWith('f') || columnName.StartsWith('n'))
        {
            return columnName[1..];
        }

        throw new Exception(UnknownColumnFormatError);
    }
}