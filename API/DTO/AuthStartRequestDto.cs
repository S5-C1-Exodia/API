namespace API.DTO;

using System;
using System.Collections.Generic;

public class AuthStartRequestDto
{
    private IList<string> _scopes;

    public AuthStartRequestDto(IList<string> scopes)
    {
        this.Scopes = scopes;
    }

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