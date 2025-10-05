using MySqlConnector;

namespace API.Managers.InterfacesServices
{
    /// <summary>
    /// Abstraction pour exécuter une unité de travail dans une transaction MySQL.
    /// </summary>
    public interface ITransactionRunner
    {
        Task RunInTransaction(Func<MySqlConnection, MySqlTransaction, Task> work);
    }
}