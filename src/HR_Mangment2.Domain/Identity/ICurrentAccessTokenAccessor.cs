namespace HR_Mangment2.Identity;

/// <summary>The stored-token id of the access token on the current request, or null.</summary>
public interface ICurrentAccessTokenAccessor
{
    string? GetTokenId();
}
