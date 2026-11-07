using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HR_Mangment2.Evaluations;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace HR_Mangment2.Controllers.Evaluations;

public class EvaluationCycleController : HR_Mangment2Controller
{
    private readonly IEvaluationCycleAppService _service;

    public EvaluationCycleController(IEvaluationCycleAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/evaluation-cycle/current", Name = "EvaluationCycle_GetCurrent")]
    public Task<EvaluationCycleDto> GetCurrentAsync() => _service.GetCurrentAsync();

    [HttpGet("api/app/evaluation-cycle", Name = "EvaluationCycle_GetList")]
    public Task<EvaluationCyclePagedResultDto> GetListAsync([FromQuery] EvaluationCycleListInput input) => _service.GetListAsync(input);
}

public class EvaluationController : HR_Mangment2Controller
{
    private readonly IEvaluationAppService _service;

    public EvaluationController(IEvaluationAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/evaluation/team", Name = "Evaluation_GetTeamList")]
    public Task<EvaluationManagerSummaryPagedResultDto> GetTeamListAsync([FromQuery] EvaluationTeamListInput input) => _service.GetTeamListAsync(input);

    [HttpGet("api/app/evaluation/{id:guid}", Name = "Evaluation_GetManagerForm")]
    public Task<EvaluationManagerDto> GetManagerFormAsync(Guid id) => _service.GetManagerFormAsync(id);

    [HttpPut("api/app/evaluation/{id:guid}", Name = "Evaluation_SaveDraft")]
    public Task<EvaluationManagerDto> SaveDraftAsync(Guid id, [FromBody] EvaluationSaveDraftDto input) => _service.SaveDraftAsync(id, input);

    [HttpPost("api/app/evaluation/{id:guid}/submit", Name = "Evaluation_Submit")]
    public Task<EvaluationManagerDto> SubmitAsync(Guid id, [FromBody] EvaluationSubmitInputDto input) => _service.SubmitAsync(id, input);

    [HttpGet("api/app/evaluation/my", Name = "Evaluation_GetMyList")]
    public Task<EvaluationEmployeeSummaryPagedResultDto> GetMyListAsync([FromQuery] EvaluationMyListInput input) => _service.GetMyListAsync(input);

    [HttpGet("api/app/evaluation/my/{id:guid}", Name = "Evaluation_GetMyDetail")]
    public Task<EvaluationEmployeeDto> GetMyDetailAsync(Guid id) => _service.GetMyDetailAsync(id);

    [HttpPost("api/app/evaluation/{id:guid}/acknowledge", Name = "Evaluation_Acknowledge")]
    public Task<EvaluationEmployeeDto> AcknowledgeAsync(Guid id) => _service.AcknowledgeAsync(id);

    [HttpPost("api/app/evaluation/{id:guid}/response", Name = "Evaluation_AddResponse")]
    public async Task<ActionResult<EvaluationResponseDto>> AddResponseAsync(
        Guid id,
        [FromBody] EvaluationResponseCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.AddResponseAsync(id, input, idempotencyKey));

    [HttpPost("api/app/evaluation/{id:guid}/response/{rid:guid}/decide", Name = "Evaluation_DecideObjection")]
    public Task<EvaluationHrDto> DecideObjectionAsync(Guid id, Guid rid, [FromBody] EvaluationObjectionDecideDto input) => _service.DecideObjectionAsync(id, rid, input);

    [HttpGet("api/app/evaluation", Name = "Evaluation_GetHrList")]
    public Task<EvaluationHrSummaryPagedResultDto> GetHrListAsync([FromQuery] EvaluationHrListInput input) => _service.GetHrListAsync(input);

    [HttpGet("api/app/evaluation/{id:guid}/hr-view", Name = "Evaluation_GetHrView")]
    public Task<EvaluationHrDto> GetHrViewAsync(Guid id) => _service.GetHrViewAsync(id);

    [HttpGet("api/app/evaluation-annual-summary/my", Name = "EvaluationAnnualSummary_GetMy")]
    public Task<EvaluationAnnualSummaryDto> GetMyAnnualSummaryAsync([FromQuery, Required, Range(2000, 2100)] int? year) => _service.GetMyAnnualSummaryAsync(year!.Value);

    [HttpGet("api/app/evaluation-annual-summary/employee/{employeeId:guid}", Name = "EvaluationAnnualSummary_GetForEmployee")]
    public Task<EvaluationAnnualSummaryDto> GetAnnualSummaryAsync(Guid employeeId, [FromQuery, Required, Range(2000, 2100)] int? year) => _service.GetAnnualSummaryAsync(employeeId, year!.Value);
}

public class EvaluationInternalNoteController : HR_Mangment2Controller
{
    private readonly IEvaluationInternalNoteAppService _service;

    public EvaluationInternalNoteController(IEvaluationInternalNoteAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/evaluation/{id:guid}/internal-note", Name = "EvaluationInternalNote_GetList")]
    public Task<EvaluationInternalNotePagedResultDto> GetListAsync(Guid id, [FromQuery] PagedAndSortedResultRequestDto input) => _service.GetListAsync(id, input);

    [HttpPost("api/app/evaluation/{id:guid}/internal-note", Name = "EvaluationInternalNote_Add")]
    public async Task<ActionResult<EvaluationInternalNoteDto>> AddAsync(
        Guid id,
        [FromBody] EvaluationInternalNoteCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.AddAsync(id, input, idempotencyKey));

    [HttpPut("api/app/evaluation/{id:guid}/internal-note/draft", Name = "EvaluationInternalNote_SaveDraft")]
    public Task<EvaluationInternalNoteDto> SaveDraftAsync(Guid id, [FromBody] EvaluationInternalNoteDraftDto input) => _service.SaveDraftAsync(id, input);
}

public class EvaluationCriterionController : HR_Mangment2Controller
{
    private readonly IEvaluationCriterionAppService _service;

    public EvaluationCriterionController(IEvaluationCriterionAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/evaluation-criterion", Name = "EvaluationCriterion_GetList")]
    public Task<EvaluationCriterionPagedResultDto> GetListAsync([FromQuery] EvaluationScaleListInput input) => _service.GetListAsync(input);

    [HttpPost("api/app/evaluation-criterion", Name = "EvaluationCriterion_Create")]
    public async Task<ActionResult<EvaluationCriterionDto>> CreateAsync([FromBody] EvaluationCriterionCreateDto input) =>
        StatusCode(201, await _service.CreateAsync(input));

    [HttpPut("api/app/evaluation-criterion/{id:guid}", Name = "EvaluationCriterion_Update")]
    public Task<EvaluationCriterionDto> UpdateAsync(Guid id, [FromBody] EvaluationCriterionUpdateDto input) => _service.UpdateAsync(id, input);
}

public class EvaluationGradeController : HR_Mangment2Controller
{
    private readonly IEvaluationGradeAppService _service;

    public EvaluationGradeController(IEvaluationGradeAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/evaluation-grade", Name = "EvaluationGrade_GetList")]
    public Task<EvaluationGradePagedResultDto> GetListAsync([FromQuery] EvaluationScaleListInput input) => _service.GetListAsync(input);

    [HttpPost("api/app/evaluation-grade", Name = "EvaluationGrade_Create")]
    public async Task<ActionResult<EvaluationGradeDto>> CreateAsync([FromBody] EvaluationGradeCreateDto input) =>
        StatusCode(201, await _service.CreateAsync(input));

    [HttpPut("api/app/evaluation-grade/{id:guid}", Name = "EvaluationGrade_Update")]
    public Task<EvaluationGradeDto> UpdateAsync(Guid id, [FromBody] EvaluationGradeUpdateDto input) => _service.UpdateAsync(id, input);
}

public class DepartmentReportController : HR_Mangment2Controller
{
    private readonly IDepartmentReportAppService _service;

    public DepartmentReportController(IDepartmentReportAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/department-report", Name = "DepartmentReport_GetList")]
    public Task<DepartmentReportSummaryPagedResultDto> GetListAsync([FromQuery] DepartmentReportListInput input) => _service.GetListAsync(input);

    [HttpGet("api/app/department-report/{id:guid}", Name = "DepartmentReport_Get")]
    public Task<DepartmentReportDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPatch("api/app/department-report/{id:guid}", Name = "DepartmentReport_UpdateNotes")]
    public Task<DepartmentReportDto> UpdateNotesAsync(Guid id, [FromBody] DepartmentReportNotesPatchDto input) => _service.UpdateNotesAsync(id, input);

    [HttpPost("api/app/department-report/{id:guid}/submit", Name = "DepartmentReport_Submit")]
    public Task<DepartmentReportDto> SubmitAsync(Guid id, [FromBody] DepartmentReportSubmitDto input) => _service.SubmitAsync(id, input);
}

public class CircularController : HR_Mangment2Controller
{
    private readonly ICircularAppService _service;

    public CircularController(ICircularAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/circular", Name = "Circular_GetReceivedList")]
    public Task<CircularReceivedPagedResultDto> GetReceivedListAsync([FromQuery] CircularReceivedListInput input) => _service.GetReceivedListAsync(input);

    [HttpPost("api/app/circular", Name = "Circular_Create")]
    public async Task<ActionResult<CircularSentDto>> CreateAsync([FromBody] CircularCreateDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("api/app/circular/sent", Name = "Circular_GetSentList")]
    public Task<CircularSentPagedResultDto> GetSentListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _service.GetSentListAsync(input);

    [HttpGet("api/app/circular/{id:guid}", Name = "Circular_Get")]
    public Task<CircularReceivedDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpGet("api/app/circular/{id:guid}/receipt", Name = "Circular_GetReceipts")]
    public Task<CircularReceiptPagedResultDto> GetReceiptsAsync(Guid id, [FromQuery] CircularReceiptListInput input) => _service.GetReceiptsAsync(id, input);

    [HttpPost("api/app/circular/{id:guid}/acknowledge", Name = "Circular_Acknowledge")]
    public Task<CircularReceivedDto> AcknowledgeAsync(Guid id) => _service.AcknowledgeAsync(id);
}
