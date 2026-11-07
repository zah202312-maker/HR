using System.Threading.Tasks;
using HR_Mangment2.Auditing;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.Auditing;

[Route("api/app/audit-log")]
public class AuditLogController : HR_Mangment2Controller
{
    private readonly IAuditLogAppService _service;

    public AuditLogController(IAuditLogAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "AuditLog_List")]
    public Task<AuditLogEntryPagedResultDto> GetListAsync([FromQuery] AuditLogListInput input) => _service.GetListAsync(input);
}
