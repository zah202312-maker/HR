using System;
using System.Threading.Tasks;
using HR_Mangment2.ReviewReports;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.ReviewReports;

public class ReviewReportController : HR_Mangment2Controller
{
    private readonly IReviewReportAppService _service;

    public ReviewReportController(IReviewReportAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/review-report", Name = "ReviewReport_GetList")]
    public Task<ReviewReportSummaryPagedResultDto> GetListAsync([FromQuery] ReviewReportListInput input) => _service.GetListAsync(input);

    [HttpPost("api/app/review-report", Name = "ReviewReport_Create")]
    public async Task<ActionResult<ReviewReportDto>> CreateAsync([FromBody] ReviewReportCreateDto input) =>
        StatusCode(201, await _service.CreateAsync(input));

    [HttpGet("api/app/review-report/{id:guid}", Name = "ReviewReport_Get")]
    public Task<ReviewReportDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPut("api/app/review-report/{id:guid}", Name = "ReviewReport_Update")]
    public Task<ReviewReportDto> UpdateAsync(Guid id, [FromBody] ReviewReportUpdateDto input) => _service.UpdateAsync(id, input);

    [HttpDelete("api/app/review-report/{id:guid}", Name = "ReviewReport_Delete")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("api/app/review-report/{id:guid}/submit", Name = "ReviewReport_Submit")]
    public Task<ReviewReportDto> SubmitAsync(Guid id, [FromBody] ReviewReportSubmitDto input) => _service.SubmitAsync(id, input);
}
