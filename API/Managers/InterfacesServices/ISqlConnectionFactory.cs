using System.Data.Common;

namespace API.Managers.InterfacesServices;

/// <summary>
/// Provides a method to create database connections in a provider-agnostic way.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Creates a new <see cref="DbConnection"/>.
    /// </summary>
    /// <returns>A new <see cref="DbConnection"/> instance.</returns>
    DbConnection Create();

    /// <summary>
    /// Crée et ouvre de manière asynchrone une nouvelle <see cref="DbConnection"/>.
    /// </summary>
    /// <param name="ct">Jeton d'annulation optionnel pour interrompre l'opération d'ouverture.</param>
    /// <returns>Une <see cref="Task{DbConnection}"/> représentant l'opération asynchrone, contenant la connexion ouverte.</returns>
    Task<DbConnection> CreateOpenAsync(CancellationToken ct = default);
}