namespace API.Managers.InterfacesServices;

/// <summary>
/// Interface defining hashing services.
/// Provides a method to compute the SHA-256 hash of a string and return it as a Base64-encoded string.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Computes the SHA-256 hash of the specified input string and returns the result as a Base64-encoded string.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>
    /// A Base64-encoded string representing the SHA-256 hash of the input.
    /// </returns>
    string Sha256Base64(string input);
}