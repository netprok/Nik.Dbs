namespace Nik.Dbs;

public sealed class InsertStatementGenerator(
    ITextFileWriter textFileWriter,
    IRandomTextGenerator randomTextGenerator,
    ISchemaTableGenerater schemaTableGenerater,
    IFieldGenerater fieldGenerater) : IInsertStatementGenerator
{
    private const string SqlExtension = ".sql";

    public async Task CreateAsync(InsertStatementDefinitions definitions)
    {
        var connectionString = Context.Configuration.GetConnectionString(definitions.ConnectionStringName);

        foreach (var table in definitions.Tables)
        {
            var schemaTable = await schemaTableGenerater.GenerateAsync(connectionString!, table.TableName);
            var classContent = GenerateInsertStatement(schemaTable, table, definitions);
            var fileName = Path.Combine(definitions.OutputPath, "insert_" + table.FullTableName + SqlExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            await textFileWriter.WriteAsync(fileName, classContent);
        }
    }

    private string GenerateInsertStatement(DataTable schemaTable, InsertStatementDefinitions.Table table, InsertStatementDefinitions definitions)
    {
        var fields = fieldGenerater.Generate(schemaTable, table).Where(field =>
            !field.IsIdentity &&
            field.DataType != "timestamp" &&
            (definitions.IncludeNullable || !field.IsNullable));

        var columnNames = GenerateColumnNames(fields);

        List<string> values;
        if (definitions.GenerateRandomValue)
        {
            Random random = new();
            values = fields.Select(field => GenerateRandomValue(field, random)).ToList();
        }
        else
        {
            values = GenerateParameters(fields);
        }

        return $"""
            INSERT INTO {table.FullTableName} (
            {string.Join(Environment.NewLine + ",", columnNames)}
            ) VALUES (
            {string.Join(Environment.NewLine + ",", values)}
            )
            """;
    }

    private static List<string> GenerateColumnNames(IEnumerable<Field> fields)
    {
        return fields.Select(field => field.ColumnName).ToList();
    }

    private List<string> GenerateParameters(IEnumerable<Field> fields)
    {
        return fields.Select(field => $"@{field.ColumnName}").ToList();
    }

    private string GenerateRandomValue(Field field, Random random)
    {
        if (new Type[] { typeof(byte), typeof(int), typeof(decimal), typeof(float), typeof(double) }.Contains(field.PropertyType))
        {
            return "1";
        }
        else if (field.PropertyType == typeof(string) || field.PropertyType == typeof(char))
        {
            return $"'{randomTextGenerator.GenerateRandomText(1)}'";
        }
        else if (field.PropertyType == typeof(bool))
        {
            return $"{random.Next(2)}";
        }
        else if (field.PropertyType == typeof(DateTime))
        {
            DateTime startDate = new DateTime(2000, 1, 1);
            int range = (DateTime.Today - startDate).Days;
            return $"'{startDate.AddDays(random.Next(range)).ToString("s")}'";
        }
        else if (field.DataType == "timestamp")
        {
            return $"CURRENT_TIMESTAMP";
        }
        else
        {
            throw new NotSupportedException($"Type '{field.PropertyType.Name}' is not supported for random value generation.");
        }
    }

    private static int GetMaxLength(PropertyInfo property)
    {
        var maxLengthAttribute = property.GetCustomAttribute<MaxLengthAttribute>();
        return maxLengthAttribute != null ? maxLengthAttribute.Length : 1;
    }
}