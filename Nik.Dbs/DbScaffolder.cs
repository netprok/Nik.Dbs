namespace Nik.Dbs;

public class DbScaffolder(
    ITextFileWriter textFileWriter,
    IScaffoldPropertyNameGenerater scaffoldPropertyNameGenerater) : IDbScaffolder
{
    private const string CsExtension = ".cs";
    private const string DataTypeField = "DATA_TYPE";
    private const string ColumnNameField = "COLUMN_NAME";

    public async Task ScaffoldAsync(ScaffoldDefinition scaffoldDefinition)
    {
        var connectionString = Context.Configuration.GetConnectionString(scaffoldDefinition.ConnectionStringName);
        using SqlConnection connection = new(connectionString);

        await connection.OpenAsync();

        foreach (var table in scaffoldDefinition.Tables)
        {
            var classContent = GenerateTableClass(connection, table);
            await textFileWriter.WriteAsync(Path.Combine(scaffoldDefinition.OutputPath, table.ClassName + CsExtension), classContent);
        }

        connection.Close();
    }

    private string GenerateTableClass(SqlConnection connection, ScaffoldDefinitionTable table)
    {
        var schemaTable = connection.GetSchema("Columns", [null, null, table.TableName]);

        StringBuilder stringBuilder = new StringBuilder();

        // generating namespace and class declaration
        stringBuilder.AppendLine("using System;");
        stringBuilder.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("namespace GeneratedModels");
        stringBuilder.AppendLine("{");

        // adding table attribute to the class
        stringBuilder.AppendLine($"    [Table(\"{table.TableName}\")]");
        stringBuilder.AppendLine($"    public class {table.ClassName}Model");
        stringBuilder.AppendLine("    {");

        // generating properties for each column
        foreach (DataRow row in schemaTable.Rows)
        {
            string columnName = row[ColumnNameField].ToString() ?? throw new ArgumentNullException("ColumnName");
            var propertyName = scaffoldPropertyNameGenerater.Generate(columnName);

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                continue;
            }

            var dataType = row[DataTypeField].ToString();
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
            stringBuilder.AppendLine($"        public {GetCSharpType(dataType)} {propertyName} {{ get; set; }}");
            stringBuilder.AppendLine();
        }

        // closing class declaration
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();
    }

    private string GetCSharpType(string dataType, int length = 0)
    {
        var type = dataType switch
        {
            "int" => "int",
            "tinyint" => "short",
            "bit" => "bool",
            "char" => "string",
            "nchar" => "string",
            "nvarchar" => "string",
            "varchar" => "string",
            "datetime" => "DateTime",
            "decimal" => "decimal",
            "float" => "double",
            "real" => "float",
            _ => "object",
        };

        if (length == 1 && dataType == "char")
        {
            type = "char";
        }

        return type;
    }
}