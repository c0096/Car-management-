using System.Text;
using Orders.Entity;

namespace Orders.Service;

public sealed class PdfReportService : IPdfReportService
{
    private const double PageWidth = 595;
    private const double PageHeight = 842;
    private const double Margin = 28;
    private const double ContentWidth = PageWidth - Margin * 2;
    private const double SectionHeaderHeight = 18;
    private const double DefaultRowHeight = 31;

    public byte[] Generate(Order order)
    {
        return BuildPdf(BuildPageContent(order));
    }

    private static string BuildPageContent(Order order)
    {
        var builder = new StringBuilder();
        var y = PageHeight - Margin;

        DrawRectangle(builder, Margin, Margin, ContentWidth, PageHeight - Margin * 2);
        DrawCenteredText(builder, "F2", 16, y - 8, "DECLARATION DE VENTE DE VEHICULE");
        DrawCenteredText(builder, "F1", 8, y - 23, "Formulaire administratif officiel");
        y -= 40;

        y = DrawTopSummary(builder, y, order);
        y -= 6;
        y = DrawSection(builder, y, "Informations du redacteur", 2, DefaultRowHeight,
        [
            new FieldValue("Nom du redacteur", order.WriterName),
            new FieldValue("Numero autorisation", order.AuthorizationNumber),
            new FieldValue("Telephone", order.WriterPhone),
            new FieldValue("Ville", order.City),
            new FieldValue("Date / Heure", order.OrderDateTime.ToString("dd/MM/yyyy HH:mm"))
        ]);
        y -= 6;
        y = DrawSection(builder, y, "Informations du vendeur", 2, DefaultRowHeight,
        [
            new FieldValue("Nom du vendeur", order.SellerName),
            new FieldValue("Adresse", order.SellerAddress),
            new FieldValue("N CIN du vendeur", order.SellerCin),
            new FieldValue("Telephone", order.SellerPhone)
        ]);
        y -= 6;
        y = DrawSection(builder, y, "Order et vehicule", 3, DefaultRowHeight,
        [
            new FieldValue("Declare avoir vendu", order.SoldItemDescription),
            new FieldValue("Numero d'ordre", order.OrderNumber),
            new FieldValue("Titre de propriete", order.PropertyTitle),
            new FieldValue("Type", order.VehicleType),
            new FieldValue("Marque", order.VehicleBrand),
            new FieldValue("Numero chassis", order.ChassisNumber)
        ]);
        y -= 6;
        y = DrawSection(builder, y, "Informations de l'acheteur", 2, DefaultRowHeight,
        [
            new FieldValue("Nom de l'acheteur", order.BuyerName),
            new FieldValue("Adresse", order.BuyerAddress),
            new FieldValue("N CIN de l'acheteur", order.BuyerCin),
            new FieldValue("Telephone", order.BuyerPhone)
        ]);
        y -= 6;
        y = DrawSection(builder, y, "Documents et observation", 1, 35,
        [
            new FieldValue("Observation", string.IsNullOrWhiteSpace(order.Observation) ? "Aucune" : order.Observation),
            new FieldValue("Pieces jointes", SummarizeAttachments(order.Attachments))
        ], 2);
        y -= 8;
        DrawSignatures(builder, y, order);

        return builder.ToString();
    }

    private static double DrawTopSummary(StringBuilder builder, double top, Order order)
    {
        var rowHeight = 27;
        var bottom = top - rowHeight;
        var cellWidth = ContentWidth / 3;

        DrawFilledRectangle(builder, Margin, bottom, ContentWidth, rowHeight, "0.93 0.96 1");
        DrawRectangle(builder, Margin, bottom, ContentWidth, rowHeight);
        DrawCell(builder, Margin, bottom, cellWidth, rowHeight, "Numero d'ordre", order.OrderNumber, 1);
        DrawCell(builder, Margin + cellWidth, bottom, cellWidth, rowHeight, "Ville", order.City, 1);
        DrawCell(builder, Margin + cellWidth * 2, bottom, cellWidth, rowHeight, "Date / Heure", order.OrderDateTime.ToString("dd/MM/yyyy HH:mm"), 1);

        return bottom;
    }

    private static double DrawSection(StringBuilder builder, double top, string title, int columns, double rowHeight, IReadOnlyList<FieldValue> fields, int maxValueLines = 1)
    {
        var rows = (int)Math.Ceiling(fields.Count / (double)columns);
        var height = SectionHeaderHeight + rows * rowHeight;
        var bottom = top - height;
        var headerBottom = top - SectionHeaderHeight;

        DrawRectangle(builder, Margin, bottom, ContentWidth, height);
        DrawFilledRectangle(builder, Margin, headerBottom, ContentWidth, SectionHeaderHeight, "0.10 0.25 0.55");
        DrawText(builder, "F2", 8.5, Margin + 8, headerBottom + 6, title.ToUpperInvariant(), "1 1 1");

        var cellWidth = ContentWidth / columns;

        for (var index = 0; index < fields.Count; index++)
        {
            var row = index / columns;
            var column = index % columns;
            var x = Margin + column * cellWidth;
            var cellTop = headerBottom - row * rowHeight;
            var cellBottom = cellTop - rowHeight;
            DrawCell(builder, x, cellBottom, cellWidth, rowHeight, fields[index].Label, fields[index].Value, maxValueLines);
        }

        return bottom;
    }

    private static void DrawCell(StringBuilder builder, double x, double y, double width, double height, string label, string value, int maxValueLines)
    {
        DrawRectangle(builder, x, y, width, height);
        DrawText(builder, "F2", 6.8, x + 5, y + height - 10, label);

        var lines = WrapValue(value, width - 10, 7.4, maxValueLines).ToArray();

        for (var index = 0; index < lines.Length; index++)
        {
            DrawText(builder, "F1", 7.4, x + 5, y + height - 22 - index * 9, lines[index]);
        }
    }

    private static void DrawSignatures(StringBuilder builder, double top, Order order)
    {
        var height = 76;
        var bottom = top - height;
        var cellWidth = ContentWidth / 3;

        DrawRectangle(builder, Margin, bottom, ContentWidth, height);
        DrawFilledRectangle(builder, Margin, top - SectionHeaderHeight, ContentWidth, SectionHeaderHeight, "0.10 0.25 0.55");
        DrawText(builder, "F2", 8.5, Margin + 8, top - SectionHeaderHeight + 6, "SIGNATURES", "1 1 1");
        DrawSignatureCell(builder, Margin, bottom, cellWidth, height - SectionHeaderHeight, "Signature du vendeur", order.SellerSignature);
        DrawSignatureCell(builder, Margin + cellWidth, bottom, cellWidth, height - SectionHeaderHeight, "Signature du gerant", order.ManagerSignature);
        DrawSignatureCell(builder, Margin + cellWidth * 2, bottom, cellWidth, height - SectionHeaderHeight, "Signature de l'acheteur", order.BuyerSignature);
        DrawText(builder, "F1", 6.5, Margin, bottom - 12, $"Document genere le {DateTime.Now:dd/MM/yyyy HH:mm}");
    }

    private static void DrawSignatureCell(StringBuilder builder, double x, double y, double width, double height, string label, string value)
    {
        DrawRectangle(builder, x, y, width, height);
        DrawText(builder, "F2", 7, x + 5, y + height - 12, label);
        DrawText(builder, "F1", 7.5, x + 5, y + 12, Shorten(value, 35));
        DrawLine(builder, x + 8, y + 22, x + width - 8, y + 22);
    }

    private static void DrawCenteredText(StringBuilder builder, string font, double size, double y, string text)
    {
        var normalized = NormalizePdfText(text);
        var estimatedWidth = normalized.Length * size * 0.48;
        DrawText(builder, font, size, (PageWidth - estimatedWidth) / 2, y, normalized);
    }

    private static void DrawText(StringBuilder builder, string font, double size, double x, double y, string text, string color = "0 0 0")
    {
        builder.Append(CultureInvariant($"{color} rg BT /{font} {size:0.##} Tf {x:0.##} {y:0.##} Td ({Escape(text)}) Tj ET\n"));
    }

    private static void DrawRectangle(StringBuilder builder, double x, double y, double width, double height)
    {
        builder.Append(CultureInvariant($"0.40 0.46 0.55 RG 0.45 w {x:0.##} {y:0.##} {width:0.##} {height:0.##} re S\n"));
    }

    private static void DrawFilledRectangle(StringBuilder builder, double x, double y, double width, double height, string color)
    {
        builder.Append(CultureInvariant($"q {color} rg {x:0.##} {y:0.##} {width:0.##} {height:0.##} re f Q\n"));
    }

    private static void DrawLine(StringBuilder builder, double x1, double y1, double x2, double y2)
    {
        builder.Append(CultureInvariant($"0.40 0.46 0.55 RG 0.45 w {x1:0.##} {y1:0.##} m {x2:0.##} {y2:0.##} l S\n"));
    }

    private static byte[] BuildPdf(string content)
    {
        var offsets = new List<long> { 0 };
        using var stream = new MemoryStream();

        WriteAscii(stream, "%PDF-1.4\n");
        WriteObject(stream, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
        WriteObject(stream, offsets, 2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        WriteObject(stream, offsets, 3, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 5 0 R /F2 6 0 R >> >> /Contents 4 0 R >>");
        WriteStreamObject(stream, offsets, 4, content);
        WriteObject(stream, offsets, 5, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        WriteObject(stream, offsets, 6, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

        var xrefOffset = stream.Position;
        WriteAscii(stream, "xref\n0 7\n");
        WriteAscii(stream, "0000000000 65535 f \n");

        foreach (var offset in offsets.Skip(1))
        {
            WriteAscii(stream, $"{offset:0000000000} 00000 n \n");
        }

        WriteAscii(stream, $"trailer\n<< /Size 7 /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        return stream.ToArray();
    }

    private static void WriteObject(Stream stream, List<long> offsets, int number, string content)
    {
        offsets.Add(stream.Position);
        WriteAscii(stream, $"{number} 0 obj\n{content}\nendobj\n");
    }

    private static void WriteStreamObject(Stream stream, List<long> offsets, int number, string content)
    {
        var bytes = Encoding.Latin1.GetBytes(content);
        offsets.Add(stream.Position);
        WriteAscii(stream, $"{number} 0 obj\n<< /Length {bytes.Length} >>\nstream\n");
        stream.Write(bytes, 0, bytes.Length);
        WriteAscii(stream, "endstream\nendobj\n");
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static IEnumerable<string> WrapValue(string? value, double width, double size, int maxLines)
    {
        var normalized = NormalizePdfText(string.IsNullOrWhiteSpace(value) ? "-" : value);
        var maxCharacters = Math.Max(10, (int)(width / (size * 0.52)));
        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var current = new StringBuilder();

        foreach (var word in words)
        {
            if (current.Length > 0 && current.Length + word.Length + 1 > maxCharacters)
            {
                lines.Add(current.ToString());
                current.Clear();

                if (lines.Count == maxLines)
                {
                    break;
                }
            }

            if (current.Length > 0)
            {
                current.Append(' ');
            }

            current.Append(word);
        }

        if (current.Length > 0 && lines.Count < maxLines)
        {
            lines.Add(current.ToString());
        }

        if (lines.Count == 0)
        {
            lines.Add("-");
        }

        if (lines.Count == maxLines && string.Join(' ', lines).Length < normalized.Length)
        {
            lines[^1] = Shorten(lines[^1], Math.Max(4, maxCharacters - 1));
        }

        return lines;
    }

    private static string Shorten(string value, int maxLength)
    {
        var normalized = NormalizePdfText(value);

        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return $"{normalized[..Math.Max(0, maxLength - 3)]}...";
    }

    private static string SummarizeAttachments(IReadOnlyList<OrderAttachment> attachments)
    {
        if (attachments.Count == 0)
        {
            return "Aucune";
        }

        var visible = attachments.Take(4).Select(attachment => $"{attachment.OriginalFileName} ({FormatFileSize(attachment.SizeBytes)})");
        var summary = string.Join("; ", visible);

        if (attachments.Count > 4)
        {
            summary = $"{summary}; +{attachments.Count - 4} autre(s)";
        }

        return summary;
    }

    private static string Escape(string value)
    {
        return NormalizePdfText(value).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }

    private static string NormalizePdfText(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            builder.Append(character switch
            {
                '’' => "'",
                'œ' => "oe",
                'Œ' => "OE",
                '€' => "EUR",
                '–' => "-",
                '—' => "-",
                _ when character <= 255 => character.ToString(),
                _ => "?"
            });
        }

        return builder.ToString();
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} o";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024d:0.##} Ko";
        }

        return $"{bytes / 1024d / 1024d:0.##} Mo";
    }

    private static string CultureInvariant(FormattableString value)
    {
        return FormattableString.Invariant(value);
    }

    private sealed record FieldValue(string Label, string Value);
}
