using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace HR_Mangment2.Identity;

public record UserAccountFilter(string? Search, Guid? RoleId, bool? IsActive, bool? MustChangePassword);

/// <summary>Account queries that need columns and joins only the database layer can express.</summary>
public interface IUserAccountQuery
{
    /// <summary>Company accounts, without the framework admin. Search also matches the name on the employee file.</summary>
    Task<IQueryable<IdentityUser>> GetQueryableAsync(UserAccountFilter filter);
}
