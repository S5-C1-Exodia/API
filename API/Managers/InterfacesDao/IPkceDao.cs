using Api.Models;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for PKCE entry data access operations.
/// </summary>
public interface IPkceDao
{
    /// <summary>
    /// Saves a PKCE entry.
    /// </summary>
    /// <param name="entry">The PKCE entry to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAsync(PkceEntry entry);

    /// <summary>
    /// Retrieves a PKCE entry by its state.
    /// </summary>
    /// <param name="state">The PKCE state value.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the <see cref="PkceEntry"/> if found; otherwise, null.
    /// </returns>
    Task<PkceEntry?> GetAsync(string state);

    /// <summary>
    /// Deletes a PKCE entry by its state.
    /// </summary>
    /// <param name="state">The PKCE state value.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(string state);
}