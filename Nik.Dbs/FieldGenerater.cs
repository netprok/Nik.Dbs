namespace Nik.Dbs;

public sealed class FieldGenerater(
    IScaffoldPropertyNameGenerater scaffoldPropertyNameGenerater) : IFieldGenerater
{
    private const string DataTypeField = "DATA_TYPE";
    private const string ColumnNameField = "COLUMN_NAME";
    private const string IsNullableName = "IS_NULLABLE";
    private const string IsIdentityName = "IS_IDENTITY";
    private const string OrdinalPositionName = "ORDINAL_POSITION";
    private const string CharacterMaximumLengthName = "CHARACTER_MAXIMUM_LENGTH";
    private const string NummericPrecisionName = "NUMERIC_PRECISION";
    private const string NumericScaleName = "NUMERIC_SCALE";

    public List<Field> Generate(DataTable schemaTable, TableBase table)
    {
        List<Field> fields = [];

        foreach (DataRow row in schemaTable.Rows)
        {
            fields.Add(GenerateField(row, table) ?? throw new ArgumentNullException("Field"));
        }

        return fields;
    }

    private Field? GenerateField(DataRow row, TableBase table)
    {
        var columnName = row[ColumnNameField].ToString() ?? throw new ArgumentNullException("ColumnName");
        var propertyName = scaffoldPropertyNameGenerater.Generate(columnName, table);

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
            PropertyName = propertyName,
            OrdinalPosition = Convert.ToInt32(row[OrdinalPositionName].ToString()),
            IsNullable = row[IsNullableName].ToString() == "True",
            IsIdentity = row[IsIdentityName].ToString() == "True",
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
            "int" => typeof(int),
            "tinyint" => typeof(byte),
            "bit" => typeof(bool),
            "char" => typeof(string),
            "nchar" => typeof(string),
            "nvarchar" => typeof(string),
            "varchar" => typeof(string),
            "datetime" => typeof(DateTime),
            "decimal" => typeof(decimal),
            "float" => typeof(double),
            "real" => typeof(float),
            "timestamp" => typeof(byte[]),
            "uniqueidentifier" => typeof(Guid),
            _ => typeof(object),
        };

        if (field.MaxLength == 1 && field.DataType == "char")
        {
            propertyType = typeof(char);
        }
         
        field.PropertyType = propertyType;
        field.PropertyTypeName = propertyType.Name switch
        {
            "Int32"=> "int",
            "Int16" => "short",
            "Boolean" => "bool",
            "String" => "string",
            "Char" => "char",
            "Decimal" => "decimal",
            "Single" => "float",
            "Double" => "double",
            "Byte" => "byte",
            "Byte[]" => "byte[]",
            _ => propertyType.Name,
        };
    }
}