using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Notices;

public class NoticeCreateDto
{
    public List<Guid> RecipientEmployeeIds { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
}

public class NoticeReceivedDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public UserRefDto Sender { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}

public class NoticeReceivedPagedResultDto
{
    public long TotalCount { get; set; }
    public List<NoticeReceivedDto> Items { get; set; } = new();
}

public class NoticeSentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public int RecipientsCount { get; set; }
    public int AcknowledgedCount { get; set; }
}

public class NoticeSentPagedResultDto
{
    public long TotalCount { get; set; }
    public List<NoticeSentDto> Items { get; set; } = new();
}

public class NoticeReceiptDto
{
    public EmployeeRefDto Recipient { get; set; } = null!;
    public DateTime? AcknowledgedAt { get; set; }
}

public class NoticeReceiptPagedResultDto
{
    public long TotalCount { get; set; }
    public List<NoticeReceiptDto> Items { get; set; } = new();
}

public class NoticeReceivedListInput : PagedAndSortedResultRequestDto
{
    public bool? IsAcknowledged { get; set; }
}

public class NoticeReceiptListInput : PagedAndSortedResultRequestDto
{
    public bool? IsAcknowledged { get; set; }
}

public interface INoticeAppService : IApplicationService
{
    Task<NoticeReceivedPagedResultDto> GetReceivedListAsync(NoticeReceivedListInput input);
    Task<NoticeSentDto> CreateAsync(NoticeCreateDto input, Guid? idempotencyKey);
    Task<NoticeSentPagedResultDto> GetSentListAsync(PagedAndSortedResultRequestDto input);
    Task<NoticeReceivedDto> GetAsync(Guid id);
    Task<NoticeReceiptPagedResultDto> GetReceiptsAsync(Guid id, NoticeReceiptListInput input);
    Task<NoticeReceivedDto> AcknowledgeAsync(Guid id);
}
