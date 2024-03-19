namespace Nik.Dbs.Models;

public class Field
{
    public string ColumnName { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public Type PropertyType { get; set; } = typeof(object);
    public int OrdinalPosition { get; set; }
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
    public int MaxLength { get; set; }
    public int NumericPrecision { get; set; }
    public int NumericScale { get; set; }
}