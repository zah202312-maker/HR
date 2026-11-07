using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace HR_Mangment2.Identity;

/// <summary>
/// x-global-rules.access-gates: computed from the account in the database on every request, never from the token.
/// </summary>
public class AccountAccessEvaluator : ITransientDependency
{
    private readonly IIdentityUserRepository _userRepository;
    private readonly IEnumerable<IMfaRequirementContributor> _mfaContributors;

    public AccountAccessEvaluator(IIdentityUserRepository userRepository, IEnumerable<IMfaRequirementContributor> mfaContributors)
    {
        _userRepository = userRepository;
        _mfaContributors = mfaContributors;
    }

    public virtual async Task<AccountAccessState> GetAsync(Guid userId)
    {
        var user = await _userRepository.FindAsync(userId, includeDetails: false);
        if (user == null)
        {
            return AccountAccessState.Missing;
        }

        var mfaRequired = await IsMfaRequiredAsync(user);
        var mustChange = user.GetMustChangePassword();

        var gate = mustChange
            ? AccessGate.PasswordChangeRequired
            : mfaRequired && !user.TwoFactorEnabled
                ? AccessGate.MfaEnrollmentRequired
                : AccessGate.None;

        return new AccountAccessState(true, user.IsActive, gate, mustChange, mfaRequired, user.TwoFactorEnabled);
    }

    public virtual async Task<bool> IsMfaRequiredAsync(IdentityUser user)
    {
        var roles = await _userRepository.GetRoleNamesAsync(user.Id);
        if (roles.Any(HrRoles.MfaRequired.Contains))
        {
            return true;
        }

        foreach (var contributor in _mfaContributors)
        {
            if (await contributor.IsRequiredAsync(user.Id))
            {
                return true;
            }
        }

        return false;
    }
}
