import 'package:flutter/widgets.dart';
import 'package:intl/intl.dart';

/// Dates shown in the user's language. date-time values arrive in UTC and are shown local.
class Fmt {
  Fmt(BuildContext context) : _lang = Localizations.localeOf(context).languageCode;
  final String _lang;

  bool get isArabic => _lang == 'ar';

  String date(DateTime? d) => d == null ? '—' : DateFormat.yMMMd(_lang).format(d);
  String dateTime(DateTime? d) => d == null ? '—' : DateFormat.yMMMd(_lang).add_Hm().format(d.toLocal());
  String time(DateTime? d) => d == null ? '—' : DateFormat.Hm(_lang).format(d.toLocal());
  String number(num? n) {
    if (n == null) return '—';
    final f = NumberFormat.decimalPattern(_lang)..maximumFractionDigits = 2;
    return f.format(n);
  }
  String weekday(DateTime d) => DateFormat.EEEE(_lang).format(d);
}
