import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/api/generated/models.dart';
import 'package:hr_app/features/eval/review_reports_page.dart';
import 'package:hr_app/features/eval/review_rooms_page.dart';

const _manager = {'id': 'c15e8f37-92ab-4d10-a7c4-6b3e1d9f0a04', 'fullName': 'Manager', 'departmentName': 'Sales'};
const _member = {'id': '2b7c9e10-4d3a-4f8e-9b21-7a6c5d4e3f20', 'fullName': 'Member', 'departmentName': 'Sales'};
const _reviewer = {'id': '91b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'displayName': 'Reviewer'};

void main() {
  test('a daily report parses with ungraded rooms and the notification values', () {
    final r = ReviewReportDto.fromJson({
      'id': '3d1f6a20-5b8c-4e71-9a02-c4e8b7d6f103',
      'reviewer': _reviewer,
      'preparedBy': _reviewer,
      'reportDate': '2026-11-11',
      'status': 'draft',
      'content': null,
      'teams': [
        {
          'room': {'id': '5a7d2c10-3e4f-4b6a-9c8d-1e2f3a4b5c6d', 'name': 'Mobile app'},
          'manager': _manager,
          'department': null,
          'members': [_member],
          'backendGrade': null,
          'backendNotes': null,
          'frontendGrade': 'veryGood',
          'frontendNotes': 'On time',
          'notes': null,
        }
      ],
      'submittedAt': null,
      'viewedAt': null,
      'concurrencyStamp': 'x',
    });
    final team = r.teams.single;
    expect(r.reportDate, DateTime(2026, 11, 11));
    expect(team.room.name, 'Mobile app');
    expect(team.members.single.fullName, 'Member');
    expect(team.backendGrade, isNull);
    expect(team.frontendGrade, ReviewerGrade.veryGood);
    expect(reviewTeamGraded(team.backendGrade, team.frontendGrade), isFalse);
    expect(reviewTeamGraded(ReviewerGrade.weak, ReviewerGrade.excellent), isTrue);
    expect(NotificationResourceType.fromJson('reviewReport'), NotificationResourceType.reviewReport);
    expect(NotificationTemplateCode.fromJson('reviewReportSubmitted'), NotificationTemplateCode.reviewReportSubmitted);
  });

  test('a grade is sent as the contract value and an empty grade as null', () {
    const input = ReviewReportTeamInputDto(roomId: 'r', backendGrade: ReviewerGrade.good, frontendGrade: null);
    final json = input.toJson();
    expect(json['backendGrade'], 'good');
    expect(json.containsKey('frontendGrade'), isTrue);
    expect(json['frontendGrade'], isNull);
    expect(reviewGradeChoices, isNot(contains(ReviewerGrade.$unknown)));
    expect(reviewGradeChoices.length, 6);
  });

  test('a room keeps only the members linked to its lead', () {
    final lead = ReviewRoomCandidateDto.fromJson({
      'manager': _manager,
      'department': {'id': '70bc8d31-9cef-41ba-a1be-05d2b19dae14', 'name': 'Sales'},
      'members': [_member],
    });
    expect(reviewRoomMembersFor(lead, [_member['id']!, 'someone-else']), [_member['id']]);
    expect(reviewRoomMembersFor(null, [_member['id']!]), isEmpty);

    final create = const ReviewRoomCreateDto(name: 'Mobile app', managerEmployeeId: 'm', memberEmployeeIds: ['a', 'b']).toJson();
    expect(create.containsKey('onBehalfOfUserId'), isFalse);
    expect(create['memberEmployeeIds'], ['a', 'b']);
  });
}
