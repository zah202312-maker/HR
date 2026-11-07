using System;
using System.Threading.Tasks;
using HR_Mangment2.Reports;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Complaints;

/// <summary>Reads levels, statuses and dates only. Nothing here needs a complaint's key or decrypts anything.</summary>
public class ReviewComplaintCounter : IReviewComplaintCounter, ITransientDependency
{
    private readonly IRepository<Complaint, Guid> _complaints;

    public ReviewComplaintCounter(IRepository<Complaint, Guid> complaints)
    {
        _complaints = complaints;
    }

    public virtual async Task<ReviewComplaintCounts> CountAsync(Guid reviewerUserId, Guid? reviewerEmployeeId, DateTime monthStartUtc, DateTime nextMonthStartUtc, DateTime now)
    {
        Task<int> SubmittedAsync(ComplaintLevel level) => _complaints.CountAsync(x =>
            x.Level == level && x.SubmittedAt >= monthStartUtc && x.SubmittedAt < nextMonthStartUtc &&
            x.ComplainantUserId != reviewerUserId &&
            x.ComplainantEmployeeId != reviewerEmployeeId &&
            (x.RespondentUserId == null || x.RespondentUserId != reviewerUserId) &&
            (x.RespondentEmployeeId == null || x.RespondentEmployeeId != reviewerEmployeeId));

        Task<int> LateAsync(ComplaintLevel level) => _complaints.CountAsync(x =>
            x.Level == level && x.Status != ComplaintStatus.Closed && x.DueAt != null && x.DueAt < now &&
            x.ComplainantUserId != reviewerUserId &&
            x.ComplainantEmployeeId != reviewerEmployeeId &&
            (x.RespondentUserId == null || x.RespondentUserId != reviewerUserId) &&
            (x.RespondentEmployeeId == null || x.RespondentEmployeeId != reviewerEmployeeId));

        return new ReviewComplaintCounts(
            await SubmittedAsync(ComplaintLevel.Confidential),
            await SubmittedAsync(ComplaintLevel.HighlyConfidential),
            await LateAsync(ComplaintLevel.Normal),
            await LateAsync(ComplaintLevel.Confidential),
            await LateAsync(ComplaintLevel.HighlyConfidential));
    }
}
