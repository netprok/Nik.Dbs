namespace Nik.Dbs.Abstractions
{
    public interface IScaffoldPropertyNameGenerater
    {
        string Generate(string columnName, string tableName);
    }
}