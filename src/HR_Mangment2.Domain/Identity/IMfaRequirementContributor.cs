using System;
using System.Threading.Tasks;

namespace HR_Mangment2.Identity;

/// <summary>Lets other modules require two-step verification, as an approved complaint handler assignment does.</summary>
public interface IMfaRequirementContributor
{
    Task<bool> IsRequiredAsync(Guid userId);
}
