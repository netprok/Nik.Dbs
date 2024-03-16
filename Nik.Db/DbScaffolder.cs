namespace Nik.Db;

public class DbScaffolder(ITextFileWriter textFileWriter) : IDbScaffolder
{
    public Task ScaffoldAsync(string connectionStringName, string tableName, string outputPath)
    {
        string classContent = GenerateClassCode(connectionStringName, tableName);
        return textFileWriter.WriteAsync(outputPath, classContent);
    }

    private string GenerateClassCode(string connectionStringName, string tableName)
    {
        var connectionString = Context.Configuration.GetConnectionString(connectionStringName);
        using SqlConnection connection = new(connectionString);

        connection.Open();
        var schemaTable = connection.GetSchema("Columns", [null, null, tableName]);

        StringBuilder stringBuilder = new StringBuilder();

        // generating namespace and class declaration
        stringBuilder.AppendLine("using System;");
        stringBuilder.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("namespace GeneratedModels");
        stringBuilder.AppendLine("{");

        // adding table attribute to the class
        stringBuilder.AppendLine($"    [Table(\"{tableName}\")]");
        stringBuilder.AppendLine($"    public class {tableName}Model");
        stringBuilder.AppendLine("    {");

        // generating properties for each column
        foreach (DataRow row in schemaTable.Rows)
        {
            var columnName = row["COLUMN_NAME"].ToString();
            if (string.IsNullOrWhiteSpace(columnName))
            {
                continue;
            }

            var dataType = row["DATA_TYPE"].ToString();
            if (string.IsNullOrWhiteSpace(dataType))
            {
                continue;
            }

            // adding column attribute to each property
            stringBuilder.AppendLine($"        [Column(\"{columnName}\")]");

            // handling string length
            if (dataType == "nvarchar")
            {
                var maxLength = int.TryParse(row["CHARACTER_MAXIMUM_LENGTH"].ToString(), out int len) ? len : 0;
                if (maxLength > 0)
                {
                    stringBuilder.AppendLine($"        [MaxLength({maxLength})]");
                }
            }

            // handling precision for numeric types
            if (dataType == "decimal" || dataType == "float" || dataType == "real")
            {
                var precision = int.TryParse(row["NUMERIC_PRECISION"].ToString(), out int prec) ? prec : 0;
                stringBuilder.AppendLine($"        [Precision({precision})]");
            }

            // generating property
            stringBuilder.AppendLine($"        public {GetCSharpType(dataType)} {columnName} {{ get; set; }}");
            stringBuilder.AppendLine();
        }

        // closing class declaration
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();
    }

    private string GetCSharpType(string dataType) => dataType switch
    {
        "int" => "int",
        "nvarchar" => "string",
        "datetime" => "DateTime",
        "decimal" => "decimal",
        "float" => "double",
        "real" => "float",
        // Add more cases as needed for other data types
        _ => "object",
    };
}