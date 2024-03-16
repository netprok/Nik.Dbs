namespace Nke.Dbs.Console.Scaffold;

public class JtlScaffoldPropertyNameGenerater : IScaffoldPropertyNameGenerater
{
    private const string UnknownColumnFormatError = "Unknown column format";
    private static readonly char[] validStartCharacters = ['k', 't', 'd', 'c', 'f', 'n', 'b'];

    public string Generate(string columnName, string tableName)
    {
        if (!validStartCharacters.Contains(columnName[0]))
        {
            throw new Exception(UnknownColumnFormatError);
        }

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

        return columnName[1..];
    }
}