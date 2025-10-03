namespace API.DTO;

using System;
using System.Collections.Generic;

/// <summary>
/// DTO for starting an authentication request with specified scopes.
/// </summary>
public class AuthStartRequestDto
{
    private IList<string> _scopes;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthStartRequestDto"/> class.
    /// </summary>
    /// <param name="scopes">The list of scopes requested for authentication.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
    public AuthStartRequestDto(IList<string> scopes)
    {
        this.Scopes = scopes;
    }

    /// <summary>
    /// Gets or sets the list of scopes requested for authentication.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if value is null or empty.</exception>
    public IList<string> Scopes
    {
        get { return this._scopes; }
        set
        {
            if (value == null || value.Count == 0)
            {
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(value));
            }
            this._scopes = value;
        }
    }
}