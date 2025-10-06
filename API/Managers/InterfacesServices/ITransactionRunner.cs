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
    }
}