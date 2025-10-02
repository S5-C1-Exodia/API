namespace Api.Managers.InterfacesHelpers;

public interface ICryptoHelper
{
    string GenerateState(int byteLength);
    void GeneratePkce(out string codeVerifier, out string codeChallenge);
}