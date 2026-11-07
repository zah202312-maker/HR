// Hand-written support for the generated client: JSON value conversions only.

/// `format: date` values travel as `yyyy-MM-dd` in company time (CONVENTIONS.md).
DateTime hrDateFromJson(String value) {
  final parts = value.split('-');
  return DateTime(int.parse(parts[0]), int.parse(parts[1]), int.parse(parts[2]));
}

String hrDateToJson(DateTime value) {
  String two(int n) => n.toString().padLeft(2, '0');
  return '${value.year.toString().padLeft(4, '0')}-${two(value.month)}-${two(value.day)}';
}

/// `format: date-time` values are UTC and end in Z.
String hrDateTimeToJson(DateTime value) => value.toUtc().toIso8601String();

/// Multipart text parts: invariant numbers, ISO dates (x-server-requirements.multipart).
String hrFormValue(Object? value) {
  if (value is DateTime) return hrDateTimeToJson(value);
  return value.toString();
}
