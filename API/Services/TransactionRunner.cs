using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.Services
{
    /// <summary>
    /// Implements a transaction runner for MySQL, managing connection, transaction lifecycle, and error handling.
    /// </summary>
    /// <param name="factory">Factory to create MySQL connections.</param>
    /// <exception cref="ArgumentNullException">Thrown if the factory is null.</exception>
    public class MySqlTransactionRunner(ISqlConnectionFactory factory) : ITransactionRunner
    {
        private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        /// <summary>
        /// Executes a unit of work within a MySQL transaction.
        /// </summary>
        /// <param name="work">A function that takes a MySQL connection and transaction, and performs the work.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Propagates exceptions thrown during the transaction.</exception>
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