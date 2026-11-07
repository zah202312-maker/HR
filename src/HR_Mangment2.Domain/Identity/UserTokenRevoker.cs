using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace HR_Mangment2.Identity;

public class UserTokenRevoker : ITransientDependency
{
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly IIdentitySessionRepository _sessionRepository;

    public UserTokenRevoker(IOpenIddictTokenManager tokenManager, IIdentitySessionRepository sessionRepository)
    {
        _tokenManager = tokenManager;
        _sessionRepository = sessionRepository;
    }

    /// <summary>Revokes every stored token and session of the user.</summary>
    public virtual async Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await foreach (var token in _tokenManager.FindBySubjectAsync(userId.ToString(), cancellationToken))
        {
            await _tokenManager.TryRevokeAsync(token, cancellationToken);
        }

        await _sessionRepository.DeleteAllAsync(userId, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Revokes other sessions and keeps the current one. All sign-ins of a user share one permanent
    /// authorization, so the current refresh token is found as the one issued with the current access token.
    /// Stored creation dates are too coarse for that, so it uses the millisecond timestamp at the start of the
    /// sequential token id. If the current token cannot be identified, every refresh token is revoked,
    /// which is the safe failure: the user only has to sign in again.
    /// </summary>
    public virtual async Task RevokeOtherSessionsAsync(Guid userId, string? currentAccessTokenId, CancellationToken cancellationToken = default)
    {
        var tokens = new List<(object Token, string? Id, string? Type)>();
        await foreach (var token in _tokenManager.FindBySubjectAsync(userId.ToString(), cancellationToken))
        {
            tokens.Add((token, await _tokenManager.GetIdAsync(token, cancellationToken), await _tokenManager.GetTypeAsync(token, cancellationToken)));
        }

        string? keepRefreshId = null;
        if (TryGetIdTimestamp(currentAccessTokenId, out var currentTime) && tokens.Any(x => x.Id == currentAccessTokenId))
        {
            keepRefreshId = tokens
                .Where(x => x.Type == OpenIddictConstants.TokenTypeIdentifiers.RefreshToken)
                .Select(x => (x.Id, HasTime: TryGetIdTimestamp(x.Id, out var time), Time: time))
                .Where(x => x.HasTime)
                .OrderBy(x => Math.Abs(x.Time - currentTime))
                .Select(x => x.Id)
                .FirstOrDefault();
        }

        foreach (var entry in tokens)
        {
            if (entry.Id != currentAccessTokenId && entry.Id != keepRefreshId)
            {
                await _tokenManager.TryRevokeAsync(entry.Token, cancellationToken);
            }
        }
    }

    private static bool TryGetIdTimestamp(string? id, out long milliseconds)
    {
        milliseconds = 0;
        var hex = id?.Replace("-", string.Empty);
        return hex is { Length: >= 12 } && long.TryParse(hex[..12], System.Globalization.NumberStyles.HexNumber, null, out milliseconds);
    }
}
