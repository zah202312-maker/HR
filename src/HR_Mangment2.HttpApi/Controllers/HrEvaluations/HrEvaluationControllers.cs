using System;
using System.Threading.Tasks;
using HR_Mangment2.HrEvaluations;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace HR_Mangment2.Controllers.HrEvaluations;

/// <summary>HrEvaluationCriterion_*, HrEvaluation_* and HrEvaluationReport_* (decision 2026-11-10).</summary>
public class HrEvaluationController : HR_Mangment2Controller
{
    private readonly IHrEvaluationCriterionAppService _criteria;
    private readonly IHrEvaluationAppService _evaluations;

    public HrEvaluationController(IHrEvaluationCriterionAppService criteria, IHrEvaluationAppService evaluations)
    {
        _criteria = criteria;
        _evaluations = evaluations;
    }

    [HttpGet("api/app/hr-evaluation-criterion", Name = "HrEvaluationCriterion_GetList")]
    public Task<HrEvaluationCriterionPagedResultDto> GetCriteriaAsync([FromQuery] HrEvaluationCriterionListInput input) => _criteria.GetListAsync(input);

    [HttpPost("api/app/hr-evaluation-criterion", Name = "HrEvaluationCriterion_Create")]
    public async Task<ActionResult<HrEvaluationCriterionDto>> CreateCriterionAsync([FromBody] HrEvaluationCriterionCreateDto input) =>
        StatusCode(201, await _criteria.CreateAsync(input));

    [HttpPut("api/app/hr-evaluation-criterion/{id:guid}", Name = "HrEvaluationCriterion_Update")]
    public Task<HrEvaluationCriterionDto> UpdateCriterionAsync(Guid id, [FromBody] HrEvaluationCriterionUpdateDto input) => _criteria.UpdateAsync(id, input);

    [HttpGet("api/app/hr-evaluation", Name = "HrEvaluation_GetList")]
    public Task<HrEvaluationPagedResultDto> GetListAsync([FromQuery] HrEvaluationListInput input) => _evaluations.GetListAsync(input);

    [HttpPost("api/app/hr-evaluation", Name = "HrEvaluation_Create")]
    public async Task<ActionResult<HrEvaluationDto>> CreateAsync([FromBody] HrEvaluationCreateDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _evaluations.CreateAsync(input, idempotencyKey));

    [HttpGet("api/app/hr-evaluation/{id:guid}", Name = "HrEvaluation_Get")]
    public Task<HrEvaluationDto> GetAsync(Guid id) => _evaluations.GetAsync(id);

    [HttpPost("api/app/hr-evaluation/{id:guid}/send", Name = "HrEvaluation_Send")]
    public async Task<ActionResult<HrEvaluationReportDto>> SendOneAsync(Guid id, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _evaluations.SendOneAsync(id, idempotencyKey));

    [HttpGet("api/app/hr-evaluation-report", Name = "HrEvaluationReport_GetList")]
    public Task<HrEvaluationReportPagedResultDto> GetReportListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _evaluations.GetReportListAsync(input);

    [HttpPost("api/app/hr-evaluation-report", Name = "HrEvaluationReport_Send")]
    public async Task<ActionResult<HrEvaluationReportDto>> SendReportAsync([FromBody] HrEvaluationReportCreateDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _evaluations.SendReportAsync(input, idempotencyKey));

    [HttpGet("api/app/hr-evaluation-report/{id:guid}", Name = "HrEvaluationReport_Get")]
    public Task<HrEvaluationReportDto> GetReportAsync(Guid id) => _evaluations.GetReportAsync(id);
}
