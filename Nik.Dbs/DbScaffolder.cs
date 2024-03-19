namespace Nik.Dbs;

public sealed class DbScaffolder(
    ITextFileWriter textFileWriter,
    ISchemaTableGenerater schemaTableGenerater,
    IFieldGenerater fieldGenerater) : IDbScaffolder
{
    private const string CsExtension = ".cs";
    private static readonly string[] decimalTypes = ["decimal", "float", "real"];
    private static readonly string[] textTypes = ["nvarchar", "varchar", "char", "nchar"];

    public async Task ScaffoldAsync(ScaffoldDefinition scaffoldDefinition)
    {
        var connectionString = Context.Configuration.GetConnectionString(scaffoldDefinition.ConnectionStringName);

        foreach (var table in scaffoldDefinition.Tables)
        {
            var schemaTable = await schemaTableGenerater.GenerateAsync(connectionString!, table.TableName);
            var classContent = GenerateTableClass(schemaTable, table, scaffoldDefinition);
            var fileName = Path.Combine(scaffoldDefinition.OutputPath, table.ClassName + CsExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            await textFileWriter.WriteAsync(fileName, classContent);
        }
    }

    private string GenerateTableClass(DataTable schemaTable, ScaffoldDefinitionTable table, ScaffoldDefinition scaffoldDefinition)
    {
        var fields = fieldGenerater.Generate(schemaTable, table.TableName);

        StringBuilder stringBuilder = new();

        if (scaffoldDefinition.AddUsings)
        {
            stringBuilder.AppendLine("""
            using System;
            using System.ComponentModel.DataAnnotations.Schema;

            """);
        }

        if (!string.IsNullOrWhiteSpace(scaffoldDefinition.Namespace))
        {
            stringBuilder.AppendLine($"""
                namespace {scaffoldDefinition.Namespace};

                """);
        }
        stringBuilder.AppendLine($"[Table(\"{table.TableName}\")]");
        stringBuilder.AppendLine($"public class {table.ClassName}");
        stringBuilder.AppendLine("{");

        foreach (var field in fields.OrderBy(field => field.OrdinalPosition))
        {
            stringBuilder.AppendLine(string.Join(Environment.NewLine, GenerateAttributes(field)));

            stringBuilder.Append($"    public {field.PropertyType.Name}");
            if (field.IsNullable)
            {
                stringBuilder.Append("?");
            }
            stringBuilder.AppendLine($" {field.PropertyName} {{ get; set; }}" + GenerateInitialization(field));
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();
    }

    private string GenerateInitialization(Field field)
    {
        string result = string.Empty;

        if (field.DataType == "timestamp")
        {
            result = " = [];";
        }
        else if (field.PropertyType == typeof(string) && !field.IsNullable)
        {
            result = " = string.Empty;";
        }

        return result;
    }

    private static List<string> GenerateAttributes(Field field)
    {
        List<string> result = [];

        result.Add($"    [Column(\"{field.ColumnName}\")]");

        if (textTypes.Contains(field.DataType))
        {
            if (field.MaxLength > 0)
            {
                result.Add($"    [MaxLength({field.MaxLength})]");
            }
        }
        else if (decimalTypes.Contains(field.DataType))
        {
            result.Add($"    [Precision({field.NumericPrecision}, {field.NumericScale})]");
        }
        else if (field.DataType == "timestamp")
        {
            result.Add($"    [Timestamp]");
        }

        return result;
    }
}