namespace Nik.Dbs.Abstractions;

public interface IFieldGenerater
{
    List<Field> Generate(DataTable schemaTable, TableBase table);
}