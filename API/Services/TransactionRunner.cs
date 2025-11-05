using System.Data.Common;
using API.Managers.InterfacesServices;

namespace API.Services;

/// <summary>
/// Implementation of <see cref="ITransactionRunner"/> for MySQL database transactions.
/// Provides methods to run asynchronous operations in a transactional context using MySQL.
/// </summary>
public class MySqlTransactionRunner(ISqlConnectionFactory factory) : ITransactionRunner
{
    private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <inheritdoc />
    public async Task RunInTransaction(Func<DbConnection, DbTransaction, Task> work)
    {
        await using DbConnection conn = await _factory.CreateOpenAsync();
        await using DbTransaction tx = await conn.BeginTransactionAsync();

        try
        {
            await work(conn, tx);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RunAsync(Func<DbConnection, DbTransaction, Task> work, CancellationToken ct = default)
    {
        await using DbConnection conn = await _factory.CreateOpenAsync(ct);
        await using DbTransaction tx = await conn.BeginTransactionAsync(ct);

        try
        {
            await work(conn, tx);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}