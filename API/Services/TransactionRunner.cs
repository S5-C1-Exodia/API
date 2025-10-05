using API.Managers.InterfacesServices;
using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.Services
{
    /// <summary>
    /// Implémentation prod : ouvre la connexion, démarre la transaction, commit/rollback.
    /// </summary>
    public class MySqlTransactionRunner(ISqlConnectionFactory factory) : ITransactionRunner
    {
        private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        public async Task RunInTransaction(Func<MySqlConnection, MySqlTransaction, Task> work)
        {
            await using var conn = _factory.Create();
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                await work(conn, (MySqlTransaction)tx);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}