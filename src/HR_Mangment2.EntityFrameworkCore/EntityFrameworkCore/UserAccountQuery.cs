using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;

namespace HR_Mangment2.EntityFrameworkCore;

public class UserAccountQuery : IUserAccountQuery, ITransientDependency
{
    private readonly IDbContextProvider<HR_Mangment2DbContext> _dbContextProvider;

    public UserAccountQuery(IDbContextProvider<HR_Mangment2DbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<IQueryable<IdentityUser>> GetQueryableAsync(UserAccountFilter filter)
    {
        var context = await _dbContextProvider.GetDbContextAsync();
        var query = context.Set<IdentityUser>().Where(u => u.UserName != HrTenantBootstrapper.AdminUserName);

        if (filter.RoleId != null)
        {
            query = query.Where(u => u.Roles.Any(r => r.RoleId == filter.RoleId));
        }

        if (filter.IsActive != null)
        {
            query = query.Where(u => u.IsActive == filter.IsActive);
        }

        if (filter.MustChangePassword != null)
        {
            query = query.Where(u => EF.Property<bool>(u, HrUserProperties.MustChangePassword) == filter.MustChangePassword);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            var employees = context.Set<Employee>();
            query = query.Where(u =>
                u.UserName.ToLower().Contains(search) ||
                (u.Email != null && u.Email.ToLower().Contains(search)) ||
                employees.Any(e => e.UserId == u.Id &&
                                   (e.FullNameAr.ToLower().Contains(search) || (e.FullNameEn != null && e.FullNameEn.ToLower().Contains(search)))));
        }

        return query;
    }
}
