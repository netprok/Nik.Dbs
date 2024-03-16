namespace Nik.Dbs;

public class DbScaffolder(
    ITextFileWriter textFileWriter,
    IScaffoldPropertyNameGenerater scaffoldPropertyNameGenerater) : IDbScaffolder
{
    private const string CsExtension = ".cs";
    private const string DataTypeField = "DATA_TYPE";
    private const string ColumnNameField = "COLUMN_NAME";
    private const string IsNullableField = "IS_NULLABLE";
    private const string OrdinalPositionName = "ORDINAL_POSITION";
    private const string CharacterMaximumLengthName = "CHARACTER_MAXIMUM_LENGTH";
    private const string NummericPrecisionName = "NUMERIC_PRECISION";
    private const string NumericScaleName = "NUMERIC_SCALE";
    private static readonly string[] decimalTypes = ["decimal", "float", "real"];
    private static readonly string[] textTypes = ["nvarchar", "varchar", "char", "nchar"];
    private static readonly string[] sqlBooleanValues = ["yes", "y"];

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

        List<Field> fields = [];

        // generating properties for each column
        foreach (DataRow row in schemaTable.Rows)
        {
            fields.Add(GenerateField(row, table.TableName) ?? throw new ArgumentNullException("Field"));
        }

        foreach (var field in fields.OrderBy(field => field.OrdinalPosition))
        {
            // adding column attribute to each property
            stringBuilder.AppendLine($"        [Column(\"{field.ColumnName}\")]");

            // handling string length
            if (textTypes.Contains(field.DataType))
            {
                if (field.MaxLength > 0)
                {
                    stringBuilder.AppendLine($"        [MaxLength({field.MaxLength})]");
                }
            }

            // handling precision for numeric types
            if (decimalTypes.Contains(field.DataType))
            {
                stringBuilder.AppendLine($"        [Precision({field.NumericPrecision}, {field.NumericScale})]");
            }

            // generating property
            stringBuilder.AppendLine($"        public {field.PropertyType} {field.PropertyName} {{ get; set; }}");
            stringBuilder.AppendLine();
        }

        // closing class declaration
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();
    }

    private Field? GenerateField(DataRow row, string tableName)
    {
        var columnName = row[ColumnNameField].ToString() ?? throw new ArgumentNullException("ColumnName");
        var propertyName = scaffoldPropertyNameGenerater.Generate(columnName, tableName);

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return null;
        }

        var dataType = row[DataTypeField].ToString();
        if (string.IsNullOrWhiteSpace(dataType))
        {
            return null;
        }

        Field field = new()
        {
            ColumnName = columnName,
            DataType = dataType,
            OrdinalPosition = Convert.ToInt32(row[OrdinalPositionName].ToString()),
            PropertyName = propertyName,
            IsNullable = sqlBooleanValues.Contains(row[IsNullableField].ToString()?.ToLower()),
            MaxLength = int.TryParse(row[CharacterMaximumLengthName].ToString(), out int len) ? len : 0,
            NumericPrecision = int.TryParse(row[NummericPrecisionName].ToString(), out int prec) ? prec : 0,
            NumericScale = int.TryParse(row[NumericScaleName].ToString(), out int scale) ? scale : 0,
        };

        GeneratePropertyType(field);

        return field;
    }

    private static void GeneratePropertyType(Field field)
    {
        var propertyType = field.DataType switch
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

        if (field.MaxLength == 1 && field.DataType == "char")
        {
            propertyType = "char";
        }

        if (field.IsNullable)
        {
            propertyType += '?';
        }

        field.PropertyType = propertyType;
    }

    private class Field
    {
        public string ColumnName { get; internal set; } = string.Empty;
        public string PropertyName { get; internal set; } = string.Empty;
        public string DataType { get; internal set; } = string.Empty;
        public string PropertyType { get; internal set; } = string.Empty;
        public int OrdinalPosition { get; internal set; }
        public bool IsNullable { get; internal set; }
        public int MaxLength { get; internal set; }
        public int NumericPrecision { get; internal set; }
        public int NumericScale { get; internal set; }
    }
}