namespace Nik.Dbs;

public sealed class InsertStatementGenerator(
    ITextFileWriter textFileWriter,
    IRandomTextGenerator randomTextGenerator,
    IFieldGenerater fieldGenerater) : IInsertStatementGenerator
{
    private const string SqlExtension = ".sql";

    public async Task CreateAsync(InsertStatementDefinitions insertStatementDefinitions)
    {
        var connectionString = Context.Configuration.GetConnectionString(insertStatementDefinitions.ConnectionStringName);
        using SqlConnection connection = new(connectionString);

        await connection.OpenAsync();

        foreach (var tableName in insertStatementDefinitions.Tables)
        {
            var classContent = GenerateInsertStatement(connection, tableName);
            var fileName = Path.Combine(insertStatementDefinitions.OutputPath, "insert_" + tableName + SqlExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            await textFileWriter.WriteAsync(fileName, classContent);
        }

        connection.Close();
    }

    private string GenerateInsertStatement(SqlConnection connection, string tableName)
    {
        var schemaTable = connection.GetSchema("Columns", [null, null, tableName]);
        var fields = fieldGenerater.Generate(schemaTable, tableName);
        var columnNames = fields.Select(p => p.ColumnName).ToList();
        Random random = new();
        var values = fields.Select(field => GenerateRandomValue(field, random)).ToList();

        return $"""
            INSERT INTO {tableName} (
            {string.Join(Environment.NewLine + ",", columnNames)}
            ) VALUES (
            {string.Join(Environment.NewLine + ",", values)}
            )
            """;
    }

    private string GenerateRandomValue(Field field, Random random)
    {
        if (new Type[] { typeof(short), typeof(int), typeof(decimal), typeof(float), typeof(double) }.Contains(field.PropertyType))
        {
            return random.Next(1, 10).ToString();
        }
        else if (field.PropertyType == typeof(string))
        {
            return $"N'{randomTextGenerator.GenerateRandomText(field.MaxLength)}'";
        }
        else if (field.PropertyType == typeof(char))
        {
            return $"'{randomTextGenerator.GenerateRandomText(1)}'";
        }
        else if (field.PropertyType == typeof(bool))
        {
            return $"{random.Next(2).ToString()}";
        }
        else if (field.PropertyType == typeof(DateTime))
        {
            DateTime startDate = new DateTime(2000, 1, 1);
            int range = (DateTime.Today - startDate).Days;
            return $"'{startDate.AddDays(random.Next(range))}'";
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