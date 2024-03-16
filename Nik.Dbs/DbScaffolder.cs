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
            var fileName = Path.Combine(scaffoldDefinition.OutputPath, table.ClassName + CsExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            await textFileWriter.WriteAsync(fileName, classContent);
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
        stringBuilder.AppendLine($"    public class {table.ClassName}");
        stringBuilder.AppendLine("    {");

        List<Field> fields = [];

        // generating properties for each column
        foreach (DataRow row in schemaTable.Rows)
        {
            fields.Add(GenerateField(row, table.TableName) ?? throw new ArgumentNullException("Field"));
        }

        foreach (var field in fields.OrderBy(field => field.OrdinalPosition))
        {
            foreach (var attribute in field.Attributes)
            {
                stringBuilder.AppendLine($"        " + attribute);
            }

            // generating property
            stringBuilder.AppendLine($"        public {field.PropertyType} {field.PropertyName} {{ get; set; }}" + field.Initialization);
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

        GenerateAttributes(field);
        GeneratePropertyType(field);
        GenerateInitialization(field);

        return field;
    }

    private void GenerateInitialization(Field field)
    {
        if (field.DataType == "timestamp")
        {
            field.Initialization = " = Array.Empty<byte>();";
        }
        else if (field.PropertyType == "string" && !field.IsNullable)
        {
            field.Initialization = " = string.Empty;";
        }
    }

    private static void GenerateAttributes(Field field)
    {
        field.Attributes.Add($"[Column(\"{field.ColumnName}\")]");

        if (textTypes.Contains(field.DataType))
        {
            if (field.MaxLength > 0)
            {
                field.Attributes.Add($"[MaxLength({field.MaxLength})]");
            }
        }
        else if (decimalTypes.Contains(field.DataType))
        {
            field.Attributes.Add($"[Precision({field.NumericPrecision}, {field.NumericScale})]");
        }
        else if (field.DataType == "timestamp")
        {
            field.Attributes.Add($"[Timestamp]");
        }
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
            "timestamp" => "byte[]",
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
        public List<string> Attributes { get; internal set; } = [];
        public string? Initialization { get; set; }
    }
}