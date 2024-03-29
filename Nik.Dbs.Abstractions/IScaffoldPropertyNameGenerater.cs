namespace Nik.Dbs.Abstractions;

public interface IScaffoldPropertyNameGenerater
{
    string Generate(string columnName, TableBase table);
}