using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.ReviewReports;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.ReviewReports;

public class ReviewRoomController : HR_Mangment2Controller
{
    private readonly IReviewRoomAppService _service;

    public ReviewRoomController(IReviewRoomAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/review-room/candidates", Name = "ReviewRoom_GetCandidates")]
    public Task<List<ReviewRoomCandidateDto>> GetCandidatesAsync() => _service.GetCandidatesAsync();

    [HttpGet("api/app/review-room", Name = "ReviewRoom_GetList")]
    public Task<ReviewRoomPagedResultDto> GetListAsync([FromQuery] ReviewRoomListInput input) => _service.GetListAsync(input);

    [HttpPost("api/app/review-room", Name = "ReviewRoom_Create")]
    public async Task<ActionResult<ReviewRoomDto>> CreateAsync([FromBody] ReviewRoomCreateDto input) =>
        StatusCode(201, await _service.CreateAsync(input));

    [HttpGet("api/app/review-room/{id:guid}", Name = "ReviewRoom_Get")]
    public Task<ReviewRoomDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPut("api/app/review-room/{id:guid}", Name = "ReviewRoom_Update")]
    public Task<ReviewRoomDto> UpdateAsync(Guid id, [FromBody] ReviewRoomUpdateDto input) => _service.UpdateAsync(id, input);

    [HttpDelete("api/app/review-room/{id:guid}", Name = "ReviewRoom_Delete")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
