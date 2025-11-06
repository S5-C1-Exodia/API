using System.Data.Common;

namespace API.Managers.InterfacesServices
{
    /// <summary>
    /// Abstraction for executing a unit of work within a database transaction.
    /// Provides methods to run asynchronous operations in a transactional context,
    /// independent of the underlying database provider.
    /// </summary>
    public interface ITransactionRunner
    {
        /// <summary>
        /// Executes the specified asynchronous work within a database transaction.
        /// </summary>
        /// <param name="work">
        /// A function that receives a <see cref="DbConnection"/> and a <see cref="DbTransaction"/>,
        /// and performs asynchronous operations within the transaction scope.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous execution of the transactional work.
        /// </returns>
        Task RunInTransaction(Func<DbConnection, DbTransaction, Task> work);

        /// <summary>
        /// Executes the specified asynchronous work within a database transaction, with cancellation support.
        /// </summary>
        /// <param name="work">
        /// A function that receives a <see cref="DbConnection"/> and a <see cref="DbTransaction"/>,
        /// and performs asynchronous operations within the transaction scope.
        /// </param>
        /// <param name="ct">
        /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous execution of the transactional work.
        /// </returns>
        Task RunAsync(Func<DbConnection, DbTransaction, Task> work, CancellationToken ct = default);
    }
}