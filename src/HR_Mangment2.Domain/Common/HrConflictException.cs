using Volo.Abp;
using Volo.Abp.ExceptionHandling;

namespace HR_Mangment2.Common;

/// <summary>An equivalent resource already exists. The contract answers this with 409 and no error code.</summary>
public class HrConflictException : UserFriendlyException, IHasHttpStatusCode
{
    public HrConflictException(string message)
        : base(message)
    {
    }

    public int HttpStatusCode => 409;
}
