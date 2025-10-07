using MySqlConnector;

namespace API.Managers.InterfacesServices
{
    /// <summary>
    /// Abstraction for executing a unit of work within a MySQL transaction.
    /// Provides a method to run asynchronous operations in a transactional context.
    /// </summary>
    public interface ITransactionRunner
    {
        /// <summary>
        /// Executes the specified asynchronous work within a MySQL transaction.
        /// </summary>
        /// <param name="work">
        /// A function that receives a <see cref="MySqlConnection"/> and <see cref="MySqlTransaction"/>,
        /// and performs asynchronous operations within the transaction.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous execution of the transactional work.
        /// </returns>
        Task RunInTransaction(Func<MySqlConnection, MySqlTransaction, Task> work);
        
        /// <summary>
        /// Runs the given work inside a transaction, with cancellation support.
        /// </summary>
        /// <param name="work">The work to be executed within the transaction.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RunAsync(Func<MySqlConnection, MySqlTransaction, Task> work, CancellationToken ct = default);

    }
}