using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.Services;

/// <inheritdoc />
public class MySqlTransactionRunner(ISqlConnectionFactory factory) : ITransactionRunner
{
    private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <inheritdoc />
    public async Task RunInTransaction(Func<MySqlConnection, MySqlTransaction, Task> work)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

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
    public async Task RunAsync(Func<MySqlConnection, MySqlTransaction, Task> work, CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

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