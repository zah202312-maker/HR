import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/api/generated/models.dart';
import 'package:hr_app/features/eval/review_reports_page.dart';

void main() {
  test('report periods follow the contract: one day, seven days, a calendar month', () {
    final start = DateTime(2026, 2, 10);
    expect(reviewPeriodRange(ReviewReportPeriodType.daily, start), (DateTime(2026, 2, 10), DateTime(2026, 2, 10)));
    expect(reviewPeriodRange(ReviewReportPeriodType.weekly, start), (DateTime(2026, 2, 10), DateTime(2026, 2, 16)));
    expect(reviewPeriodRange(ReviewReportPeriodType.monthly, start), (DateTime(2026, 2, 1), DateTime(2026, 2, 28)));
    expect(reviewPeriodRange(ReviewReportPeriodType.monthly, DateTime(2026, 12, 31)), (DateTime(2026, 12, 1), DateTime(2026, 12, 31)));
  });

  test('a review report parses with null rates and the new notification values', () {
    final r = ReviewReportDto.fromJson({
      'id': '3d1f6a20-5b8c-4e71-9a02-c4e8b7d6f103',
      'reviewer': {'id': '91b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'displayName': 'Reviewer'},
      'periodType': 'weekly',
      'periodFrom': '2026-11-01',
      'periodTo': '2026-11-07',
      'status': 'draft',
      'summary': null,
      'averageCompletion': null,
      'teams': [
        {
          'manager': {'id': 'c15e8f37-92ab-4d10-a7c4-6b3e1d9f0a04', 'fullName': 'Manager', 'departmentName': 'Sales'},
          'department': null,
          'membersCount': 8,
          'completionPercent': null,
          'notes': null,
          'projectNotes': null,
        }
      ],
      'submittedAt': null,
      'viewedAt': null,
      'concurrencyStamp': 'x',
    });
    expect(r.periodType, ReviewReportPeriodType.weekly);
    expect(r.teams.single.completionPercent, isNull);
    expect(r.teams.single.department, isNull);
    expect(NotificationResourceType.fromJson('reviewReport'), NotificationResourceType.reviewReport);
    expect(NotificationTemplateCode.fromJson('reviewReportSubmitted'), NotificationTemplateCode.reviewReportSubmitted);
  });
}
