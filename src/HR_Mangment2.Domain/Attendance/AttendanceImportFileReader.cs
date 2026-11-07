using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ClosedXML.Excel;

namespace HR_Mangment2.Attendance;

public record AttendanceImportRow(int RowNumber, string? DeviceUserId, DateTime? PunchLocalTime);

public record AttendanceImportFile(IReadOnlyList<string> MissingColumns, IReadOnlyList<AttendanceImportRow> Rows);

/// <summary>Reads the fallback file: .xlsx or .csv, with deviceUserId and punchDateTime in the first row.</summary>
public static class AttendanceImportFileReader
{
    public const string DeviceUserIdColumn = "deviceUserId";
    public const string PunchDateTimeColumn = "punchDateTime";

    /// <summary>A small zip can expand to many gigabytes, so its declared size is checked before opening it.</summary>
    private const long MaxExpandedBytes = 200 * 1024 * 1024;

    private static readonly byte[] ZipSignature = { 0x50, 0x4B };

    public static bool IsExcel(string fileName) => string.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase);

    public static bool IsCsv(string fileName) => string.Equals(Path.GetExtension(fileName), ".csv", StringComparison.OrdinalIgnoreCase);

    public static bool LooksValid(string fileName, byte[] content) =>
        content.Length > 0 && (IsCsv(fileName) || (IsExcel(fileName) && content.AsSpan().StartsWith(ZipSignature)));

    /// <exception cref="Exception">The file is damaged and cannot be read.</exception>
    public static AttendanceImportFile Read(string fileName, byte[] content) =>
        IsExcel(fileName) ? ReadExcel(content) : ReadCsv(content);

    private static AttendanceImportFile ReadExcel(byte[] content)
    {
        using (var archive = new ZipArchive(new MemoryStream(content), ZipArchiveMode.Read))
        {
            if (archive.Entries.Sum(x => x.Length) > MaxExpandedBytes)
            {
                throw new InvalidDataException("The workbook expands beyond the allowed size.");
            }
        }

        using var workbook = new XLWorkbook(new MemoryStream(content));
        var sheet = workbook.Worksheets.First();
        var lastColumn = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;

        var header = Enumerable.Range(1, lastColumn).Select(c => sheet.Cell(1, c).GetFormattedString()).ToList();
        var (missing, userColumn, timeColumn) = FindColumns(header);
        if (missing.Count > 0)
        {
            return new AttendanceImportFile(missing, Array.Empty<AttendanceImportRow>());
        }

        var rows = new List<AttendanceImportRow>();
        for (var number = 2; number <= lastRow; number++)
        {
            var userCell = sheet.Cell(number, userColumn + 1);
            var timeCell = sheet.Cell(number, timeColumn + 1);
            if (userCell.IsEmpty() && timeCell.IsEmpty())
            {
                continue;
            }

            var time = timeCell.DataType == XLDataType.DateTime
                ? TrimToMinute(timeCell.GetDateTime())
                : ParseTime(timeCell.GetFormattedString());

            rows.Add(new AttendanceImportRow(number, userCell.GetFormattedString(), time));
        }

        return new AttendanceImportFile(missing, rows);
    }

    private static AttendanceImportFile ReadCsv(byte[] content)
    {
        using var reader = new StreamReader(new MemoryStream(content), new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);

        var headerLine = reader.ReadLine() ?? string.Empty;
        var delimiter = headerLine.Contains(';') && !headerLine.Contains(',') ? ';' : ',';
        var (missing, userColumn, timeColumn) = FindColumns(SplitLine(headerLine, delimiter));
        if (missing.Count > 0)
        {
            return new AttendanceImportFile(missing, Array.Empty<AttendanceImportRow>());
        }

        var rows = new List<AttendanceImportRow>();
        var number = 1;
        while (reader.ReadLine() is { } line)
        {
            number++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var cells = SplitLine(line, delimiter);
            rows.Add(new AttendanceImportRow(
                number,
                userColumn < cells.Count ? cells[userColumn] : null,
                timeColumn < cells.Count ? ParseTime(cells[timeColumn]) : null));
        }

        return new AttendanceImportFile(missing, rows);
    }

    private static (List<string> Missing, int UserColumn, int TimeColumn) FindColumns(IReadOnlyList<string> header)
    {
        var names = header.Select(x => x.Trim()).ToList();
        var userColumn = names.FindIndex(x => x.Equals(DeviceUserIdColumn, StringComparison.OrdinalIgnoreCase));
        var timeColumn = names.FindIndex(x => x.Equals(PunchDateTimeColumn, StringComparison.OrdinalIgnoreCase));

        var missing = new List<string>();
        if (userColumn < 0)
        {
            missing.Add(DeviceUserIdColumn);
        }

        if (timeColumn < 0)
        {
            missing.Add(PunchDateTimeColumn);
        }

        return (missing, userColumn, timeColumn);
    }

    private static DateTime? ParseTime(string? text) =>
        DateTime.TryParseExact(text?.Trim(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : null;

    private static DateTime TrimToMinute(DateTime value) => new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);

    private static List<string> SplitLine(string line, char delimiter)
    {
        var cells = new List<string>();
        var current = new StringBuilder();
        var quoted = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                quoted = true;
            }
            else if (c == delimiter)
            {
                cells.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        cells.Add(current.ToString());
        return cells;
    }
}
