namespace Api.Managers.InterfacesHelpers;

/// <summary>
/// Provides cryptographic utilities for state and PKCE generation.
/// </summary>
public interface ICryptoHelper
{
    /// <summary>
    /// Generates a random state string for OAuth flows.
    /// </summary>
    /// <param name="byteLength">The number of random bytes to use.</param>
    /// <returns>A base64url-encoded random state string.</returns>
    string GenerateState(int byteLength);

    /// <summary>
    /// Generates a PKCE code verifier and code challenge.
    /// </summary>
    /// <param name="codeVerifier">The generated code verifier (output).</param>
    /// <param name="codeChallenge">The generated code challenge (output).</param>
    void GeneratePkce(out string codeVerifier, out string codeChallenge);
}