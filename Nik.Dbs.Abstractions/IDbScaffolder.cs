using Nik.Dbs.Models;

namespace Nik.Dbs.Abstractions;

public interface IDbScaffolder
{
    Task ScaffoldAsync(ScaffoldDefinition scaffoldDefinition);
}