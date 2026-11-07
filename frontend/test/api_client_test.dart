import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/api/generated/hr_api.dart';
import 'package:hr_app/api/runtime/json.dart';
import 'package:hr_app/core/auth/pkce.dart';

void main() {
  test('enums travel as camelCase strings and tolerate unknown values', () {
    expect(RequestStatus.fromJson('pendingHr'), RequestStatus.pendingHr);
    expect(RequestStatus.pendingHr.toJson(), 'pendingHr');
    expect(RequestStatus.fromJson('somethingNew'), RequestStatus.$unknown);
  });

  test('dates: date as yyyy-MM-dd, date-time as UTC', () {
    expect(hrDateToJson(DateTime(2026, 3, 5)), '2026-03-05');
    expect(hrDateFromJson('2026-03-05'), DateTime(2026, 3, 5));
    expect(hrDateTimeToJson(DateTime.utc(2026, 1, 2, 3, 4, 5)), '2026-01-02T03:04:05.000Z');
  });

  test('optional fields are omitted, required nullable fields are sent', () {
    final dto = LeaveRequestCreateDto(kind: LeaveKind.annual, startDate: DateTime(2026, 4, 1), endDate: DateTime(2026, 4, 3));
    expect(dto.toJson(), {'kind': 'annual', 'startDate': '2026-04-01', 'endDate': '2026-04-03'});
    expect(const DepartmentReportNotesPatchDto(notes: null, concurrencyStamp: 'x').toJson(), {'notes': null, 'concurrencyStamp': 'x'});
  });

  test('nullable references (allOf + nullable) parse as null', () {
    final me = MyAccountDto.fromJson({
      'userId': '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      'userName': 'admin',
      'displayName': 'Admin',
      'preferredLanguage': 'ar',
      'roleNames': ['systemAdmin'],
      'employee': null,
      'directReportCount': 0,
      'accessGate': 'mfaEnrollmentRequired',
      'mustChangePassword': false,
      'mfaRequired': true,
      'mfaEnabled': false,
    });
    expect(me.employee, isNull);
    expect(me.accessGate, AccessGate.mfaEnrollmentRequired);
  });

  test('operation metadata comes from the contract', () {
    expect(hrOperations.length, 315);
    expect(hrOperations[HrOps.Request_Approve]!.path, '/api/app/request/{id}/approve');
  });

  test('PKCE verifier and S256 challenge are URL-safe', () {
    final p = Pkce.generate();
    expect(p.verifier.length, greaterThanOrEqualTo(43));
    expect(RegExp(r'^[A-Za-z0-9_-]+$').hasMatch(p.challenge), isTrue);
  });
}
