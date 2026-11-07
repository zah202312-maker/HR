using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HR_Mangment2.Reports;

/// <summary>What a report looks like on paper. A cell is text, a whole number or a decimal, and nothing else.</summary>
public record ReportTable(string Title, string? Period, IReadOnlyList<string> Headers, IReadOnlyList<object?[]> Rows);

/// <summary>Turns a table into a file. It knows nothing about reports, so it cannot add a field the table left out.</summary>
public static class ReportFileWriter
{
    public const string PdfContentType = "application/pdf";
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>The first family present on the machine that has the character is used. Windows has the first ones, Linux needs a Noto font installed.</summary>
    private static readonly string[] FontFamilies = { "Segoe UI", "Tahoma", "Arial", "Noto Naskh Arabic", "Noto Sans Arabic", "DejaVu Sans" };

    private const int MaxSheetNameLength = 31;

    static ReportFileWriter()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // A machine without an Arabic font should give a poor file and a log line, not a failed request.
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
    }

    public static byte[] WriteExcel(ReportTable table, bool rightToLeft)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet(SheetName(table.Title));
        sheet.RightToLeft = rightToLeft;

        sheet.Cell(1, 1).SetValue(table.Title);
        sheet.Cell(1, 1).Style.Font.Bold = true;

        var row = 2;
        if (table.Period != null)
        {
            sheet.Cell(row, 1).SetValue(table.Period);
            row++;
        }

        row++;
        for (var column = 0; column < table.Headers.Count; column++)
        {
            var cell = sheet.Cell(row, column + 1);
            cell.SetValue(table.Headers[column]);
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        foreach (var values in table.Rows)
        {
            row++;
            for (var column = 0; column < values.Length; column++)
            {
                SetCell(sheet.Cell(row, column + 1), values[column]);
            }
        }

        sheet.Columns().AdjustToContents(1, 100);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] WritePdf(ReportTable table, bool rightToLeft)
    {
        var document = Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(24);
            page.DefaultTextStyle(x => x.FontFamily(FontFamilies).FontSize(9));

            if (rightToLeft)
            {
                page.ContentFromRightToLeft();
            }

            page.Header().PaddingBottom(8).Column(column =>
            {
                column.Item().Text(table.Title).FontSize(14).Bold();
                if (table.Period != null)
                {
                    column.Item().Text(table.Period);
                }
            });

            page.Content().Table(grid =>
            {
                grid.ColumnsDefinition(columns =>
                {
                    foreach (var _ in table.Headers)
                    {
                        columns.RelativeColumn();
                    }
                });

                grid.Header(header =>
                {
                    foreach (var title in table.Headers)
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Border(0.5f).Padding(3).Text(title).Bold();
                    }
                });

                foreach (var values in table.Rows)
                {
                    foreach (var value in values)
                    {
                        grid.Cell().Border(0.5f).Padding(3).Text(ToText(value));
                    }
                }
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        }));

        return document.GeneratePdf();
    }

    private static void SetCell(IXLCell cell, object? value)
    {
        // SetValue with a string stores text, so a name that starts with "=" stays a name and never becomes a formula.
        switch (value)
        {
            case null:
                break;
            case int number:
                cell.SetValue(number);
                break;
            case decimal number:
                cell.SetValue(number);
                break;
            default:
                cell.SetValue(value.ToString());
                break;
        }
    }

    private static string ToText(object? value) => value switch
    {
        null => string.Empty,
        decimal number => number.ToString("0.##", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static string SheetName(string title)
    {
        var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        var cleaned = new string(title.Where(x => !invalid.Contains(x)).ToArray()).Trim();
        return cleaned.Length <= MaxSheetNameLength ? cleaned : cleaned[..MaxSheetNameLength];
    }
}
