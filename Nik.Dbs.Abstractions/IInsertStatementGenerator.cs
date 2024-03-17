namespace Nik.Dbs.Abstractions;

public interface IInsertStatementGenerator
{
    Task CreateAsync(InsertStatementDefinitions insertStatementDefinitions);
}