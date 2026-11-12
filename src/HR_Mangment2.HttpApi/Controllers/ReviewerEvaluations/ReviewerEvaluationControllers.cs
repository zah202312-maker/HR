using System;
using System.Threading.Tasks;
using HR_Mangment2.ReviewerEvaluations;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace HR_Mangment2.Controllers.ReviewerEvaluations;

/// <summary>ReviewerEvaluationCriterion_*, ReviewerEvaluation_* and ReviewerEvaluationReport_* (decision 2026-11-11).</summary>
public class ReviewerEvaluationController : HR_Mangment2Controller
{
    private readonly IReviewerEvaluationCriterionAppService _criteria;
    private readonly IReviewerEvaluationAppService _evaluations;

    public ReviewerEvaluationController(IReviewerEvaluationCriterionAppService criteria, IReviewerEvaluationAppService evaluations)
    {
        _criteria = criteria;
        _evaluations = evaluations;
    }

    [HttpGet("api/app/reviewer-evaluation-criterion", Name = "ReviewerEvaluationCriterion_GetList")]
    public Task<ReviewerEvaluationCriterionPagedResultDto> GetCriteriaAsync([FromQuery] ReviewerEvaluationCriterionListInput input) => _criteria.GetListAsync(input);

    [HttpPost("api/app/reviewer-evaluation-criterion", Name = "ReviewerEvaluationCriterion_Create")]
    public async Task<ActionResult<ReviewerEvaluationCriterionDto>> CreateCriterionAsync([FromBody] ReviewerEvaluationCriterionCreateDto input) =>
        StatusCode(201, await _criteria.CreateAsync(input));

    [HttpPut("api/app/reviewer-evaluation-criterion/{id:guid}", Name = "ReviewerEvaluationCriterion_Update")]
    public Task<ReviewerEvaluationCriterionDto> UpdateCriterionAsync(Guid id, [FromBody] ReviewerEvaluationCriterionUpdateDto input) => _criteria.UpdateAsync(id, input);

    [HttpGet("api/app/reviewer-evaluation", Name = "ReviewerEvaluation_GetList")]
    public Task<ReviewerEvaluationPagedResultDto> GetListAsync([FromQuery] ReviewerEvaluationListInput input) => _evaluations.GetListAsync(input);

    [HttpPost("api/app/reviewer-evaluation", Name = "ReviewerEvaluation_Create")]
    public async Task<ActionResult<ReviewerEvaluationDto>> CreateAsync([FromBody] ReviewerEvaluationCreateDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _evaluations.CreateAsync(input, idempotencyKey));

    [HttpGet("api/app/reviewer-evaluation/candidate", Name = "ReviewerEvaluation_GetCandidates")]
    public Task<HR_Mangment2.Employees.EmployeeLookupPagedResultDto> GetCandidatesAsync([FromQuery] ReviewerEvaluationCandidateInput input) => _evaluations.GetCandidatesAsync(input);

    [HttpGet("api/app/reviewer-evaluation/{id:guid}", Name = "ReviewerEvaluation_Get")]
    public Task<ReviewerEvaluationDto> GetAsync(Guid id) => _evaluations.GetAsync(id);

    [HttpPost("api/app/reviewer-evaluation/{id:guid}/send", Name = "ReviewerEvaluation_Send")]
    public async Task<ActionResult<ReviewerEvaluationReportDto>> SendOneAsync(Guid id, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _evaluations.SendOneAsync(id, idempotencyKey));

    [HttpGet("api/app/reviewer-evaluation-report", Name = "ReviewerEvaluationReport_GetList")]
    public Task<ReviewerEvaluationReportPagedResultDto> GetReportListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _evaluations.GetReportListAsync(input);

    [HttpPost("api/app/reviewer-evaluation-report", Name = "ReviewerEvaluationReport_Send")]
    public async Task<ActionResult<ReviewerEvaluationReportDto>> SendReportAsync([FromBody] ReviewerEvaluationReportCreateDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _evaluations.SendReportAsync(input, idempotencyKey));

    [HttpGet("api/app/reviewer-evaluation-report/{id:guid}", Name = "ReviewerEvaluationReport_Get")]
    public Task<ReviewerEvaluationReportDto> GetReportAsync(Guid id) => _evaluations.GetReportAsync(id);
}
