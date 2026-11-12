using System;
using System.Threading.Tasks;
using HR_Mangment2.Requests;
using HR_Mangment2.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HR_Mangment2.Controllers.Store;

public class ItemController : HR_Mangment2Controller
{
    private readonly IItemAppService _service;

    public ItemController(IItemAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/item", Name = "Item_GetList")]
    public Task<ItemPagedResultDto> GetListAsync([FromQuery] ItemListInput input) => _service.GetListAsync(input);

    [HttpPost("api/app/item", Name = "Item_Create")]
    public async Task<ActionResult<ItemDto>> CreateAsync([FromBody] ItemCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("api/app/item/lookup", Name = "Item_Lookup")]
    public Task<ItemRefPagedResultDto> LookupAsync([FromQuery] ItemLookupInput input) => _service.LookupAsync(input);

    [HttpGet("api/app/item/{id:guid}", Name = "Item_Get")]
    public Task<ItemDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPut("api/app/item/{id:guid}", Name = "Item_Update")]
    public Task<ItemDto> UpdateAsync(Guid id, [FromBody] ItemUpdateInputDto input) => _service.UpdateAsync(id, input);

    [HttpDelete("api/app/item/{id:guid}", Name = "Item_Delete")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}

public class StockController : HR_Mangment2Controller
{
    private readonly IStockAppService _service;

    public StockController(IStockAppService service)
    {
        _service = service;
    }

    [HttpPost("api/app/stock/receive", Name = "Stock_Receive")]
    public async Task<ActionResult<StockMovementDto>> ReceiveAsync([FromBody] StockReceiveInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.ReceiveAsync(input, idempotencyKey));

    [HttpPost("api/app/stock/adjust", Name = "Stock_Adjust")]
    public async Task<ActionResult<StockMovementDto>> AdjustAsync([FromBody] StockAdjustInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.AdjustAsync(input, idempotencyKey));

    [HttpGet("api/app/stock/movements", Name = "Stock_GetMovements")]
    public Task<StockMovementPagedResultDto> GetMovementsAsync([FromQuery] StockMovementListInput input) => _service.GetMovementsAsync(input);
}

public class StockIssueController : HR_Mangment2Controller
{
    private readonly IStockIssueAppService _service;

    public StockIssueController(IStockIssueAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/stock-issue", Name = "StockIssue_GetList")]
    public Task<StockIssuePagedResultDto> GetListAsync([FromQuery] StockIssueListInput input) => _service.GetListAsync(input);

    [HttpGet("api/app/stock-issue/{id:guid}", Name = "StockIssue_Get")]
    public Task<StockIssueDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPost("api/app/stock-issue/{id:guid}/acknowledge", Name = "StockIssue_Acknowledge")]
    public Task<StockIssueDto> AcknowledgeAsync(Guid id) => _service.AcknowledgeAsync(id);

    [HttpPost("api/app/stock-issue/{id:guid}/mark-purchasing", Name = "StockIssue_MarkPurchasing")]
    public Task<StockIssueDto> MarkPurchasingAsync(Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] NoteInputDto? input) =>
        _service.MarkPurchasingAsync(id, input ?? new NoteInputDto());

    [HttpPost("api/app/stock-issue/{id:guid}/deliver", Name = "StockIssue_Deliver")]
    public Task<StockIssueDto> DeliverAsync(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] StockIssueDeliverInputDto? input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        _service.DeliverAsync(id, input ?? new StockIssueDeliverInputDto(), idempotencyKey);
}

public class CustodyController : HR_Mangment2Controller
{
    private readonly ICustodyAppService _service;

    public CustodyController(ICustodyAppService service)
    {
        _service = service;
    }

    [HttpPost("api/app/custody", Name = "Custody_Create")]
    public async Task<ActionResult<CustodyDto>> CreateAsync([FromBody] CustodyCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("api/app/custody", Name = "Custody_GetList")]
    public Task<CustodyPagedResultDto> GetListAsync([FromQuery] CustodyListInput input) => _service.GetListAsync(input);

    [HttpGet("api/app/custody/my", Name = "Custody_GetMy")]
    public Task<CustodyPagedResultDto> GetMyAsync([FromQuery] CustodyMyListInput input) => _service.GetMyAsync(input);

    [HttpGet("api/app/custody/team", Name = "Custody_GetTeam")]
    public Task<CustodyPagedResultDto> GetTeamAsync([FromQuery] CustodyTeamListInput input) => _service.GetTeamAsync(input);

    [HttpGet("api/app/custody/{id:guid}", Name = "Custody_Get")]
    public Task<CustodyDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPost("api/app/custody/{id:guid}/acknowledge", Name = "Custody_Acknowledge")]
    public Task<CustodyDto> AcknowledgeAsync(Guid id) => _service.AcknowledgeAsync(id);

    [HttpPost("api/app/custody/{id:guid}/return", Name = "Custody_Return")]
    public Task<CustodyDto> ReturnAsync(Guid id, [FromBody] CustodyReturnInputDto input) => _service.ReturnAsync(id, input);
}

public class CustodyCountController : HR_Mangment2Controller
{
    private readonly ICustodyCountAppService _service;

    public CustodyCountController(ICustodyCountAppService service)
    {
        _service = service;
    }

    [HttpPost("api/app/custody-count", Name = "CustodyCount_Create")]
    public async Task<ActionResult<CustodyCountDto>> CreateAsync([FromBody] CustodyCountCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("api/app/custody-count", Name = "CustodyCount_GetList")]
    public Task<CustodyCountPagedResultDto> GetListAsync([FromQuery] CustodyCountListInput input) => _service.GetListAsync(input);
}
