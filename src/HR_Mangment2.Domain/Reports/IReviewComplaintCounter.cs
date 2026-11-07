using System;
using System.Threading.Tasks;

namespace HR_Mangment2.Reports;

/// <summary>Counts only: no name, subject, department or category ever crosses this line.</summary>
public record ReviewComplaintCounts(
    int ConfidentialSubmitted,
    int HighlyConfidentialSubmitted,
    int LateNormal,
    int LateConfidential,
    int LateHighlyConfidential);

/// <summary>
/// The complaints module supplies the answer, because the main project must not reference it. Every count leaves out
/// complaints the reviewer is a party to, and the result does not reveal that anything was left out.
/// </summary>
public interface IReviewComplaintCounter
{
    Task<ReviewComplaintCounts> CountAsync(Guid reviewerUserId, Guid? reviewerEmployeeId, DateTime monthStartUtc, DateTime nextMonthStartUtc, DateTime now);
}
