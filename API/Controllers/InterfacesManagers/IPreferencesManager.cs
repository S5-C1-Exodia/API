namespace API.Controllers.InterfacesManagers
{
    /// <summary>
    /// Interface for managing user playlist preferences.
    /// Provides transactional operations and audit logging for playlist selection management.
    /// </summary>
    public interface IPreferencesManager
    {
        /// <summary>
        /// Replaces the user's playlist selection with the provided list of playlist IDs.
        /// </summary>
        /// <param name="sessionId">The user's session identifier.</param>
        /// <param name="playlistIds">The new list of playlist IDs to set as the user's selection.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ReplaceSelectionAsync(string sessionId, IReadOnlyCollection<string> playlistIds, CancellationToken ct = default);

        /// <summary>
        /// Adds the specified playlist IDs to the user's current selection.
        /// </summary>
        /// <param name="sessionId">The user's session identifier.</param>
        /// <param name="playlistIds">Playlist IDs to add to the selection.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddToSelectionAsync(string sessionId, IReadOnlyCollection<string> playlistIds, CancellationToken ct = default);

        /// <summary>
        /// Removes the specified playlist IDs from the user's current selection.
        /// </summary>
        /// <param name="sessionId">The user's session identifier.</param>
        /// <param name="playlistIds">Playlist IDs to remove from the selection.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveFromSelectionAsync(string sessionId, IReadOnlyCollection<string> playlistIds,
            CancellationToken ct = default);

        /// <summary>
        /// Clears all playlist selections for the user.
        /// </summary>
        /// <param name="sessionId">The user's session identifier.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClearSelectionAsync(string sessionId, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the current playlist selection for the user.
        /// </summary>
        /// <param name="sessionId">The user's session identifier.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>A task that returns a list of selected playlist IDs.</returns>
        Task<List<string>> GetSelectionAsync(string sessionId, CancellationToken ct = default);
    }
}