namespace Nik.Dbs;

public interface IFieldGenerater
{
    List<Field> Generate(DataTable schemaTable, TableBase table);
}