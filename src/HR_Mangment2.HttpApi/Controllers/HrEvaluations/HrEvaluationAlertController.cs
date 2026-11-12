using System;
using System.Threading.Tasks;
using HR_Mangment2.HrEvaluations;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.HrEvaluations;

/// <summary>HrEvaluationAlert_* (decision 2026-11-11).</summary>
public class HrEvaluationAlertController : HR_Mangment2Controller
{
    private readonly IHrEvaluationAlertAppService _alerts;

    public HrEvaluationAlertController(IHrEvaluationAlertAppService alerts)
    {
        _alerts = alerts;
    }

    [HttpGet("api/app/hr-evaluation-alert", Name = "HrEvaluationAlert_GetList")]
    public Task<HrEvaluationAlertPagedResultDto> GetListAsync([FromQuery] HrEvaluationAlertListInput input) => _alerts.GetListAsync(input);

    [HttpGet("api/app/hr-evaluation-alert/{id:guid}", Name = "HrEvaluationAlert_Get")]
    public Task<HrEvaluationAlertDto> GetAsync(Guid id) => _alerts.GetAsync(id);
}
