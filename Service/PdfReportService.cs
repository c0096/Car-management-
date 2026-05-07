using System.Text;
using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Service;

public sealed class PdfReportService : IPdfReportService
{
    private const int MaxLinesPerPage = 38;

    public byte[] Generate(VehicleSaleDeclaration declaration)
    {
        var lines = BuildReportLines(declaration);
        var pages = lines.Chunk(MaxLinesPerPage).Select(chunk => chunk.ToArray()).ToArray();
        return BuildPdf(pages);
    }

    private static List<ReportLine> BuildReportLines(VehicleSaleDeclaration declaration)
    {
        var lines = new List<ReportLine>
        {
            ReportLine.Title("Declaration de vente de vehicule"),
            ReportLine.Text($"Numero d'ordre: {declaration.OrderNumber}"),
            ReportLine.Text($"Ville: {declaration.City}"),
            ReportLine.Text($"Date / Heure: {declaration.DeclarationDateTime:dd/MM/yyyy HH:mm}"),
            ReportLine.Space(),
            ReportLine.Section("Informations du redacteur"),
            ReportLine.Text($"Nom du redacteur: {declaration.WriterName}"),
            ReportLine.Text($"Numero autorisation: {declaration.AuthorizationNumber}"),
            ReportLine.Text($"Telephone du redacteur: {declaration.WriterPhone}"),
            ReportLine.Space(),
            ReportLine.Section("Informations du vendeur"),
            ReportLine.Text($"Nom du vendeur: {declaration.SellerName}"),
            ReportLine.Text($"Adresse: {declaration.SellerAddress}"),
            ReportLine.Text($"CIN du vendeur: {declaration.SellerCin}"),
            ReportLine.Text($"Telephone du vendeur: {declaration.SellerPhone}"),
            ReportLine.Space(),
            ReportLine.Section("Declaration de vente"),
            ReportLine.Text($"Declare avoir vendu: {declaration.SoldItemDescription}"),
            ReportLine.Text($"Titre de propriete: {declaration.PropertyTitle}"),
            ReportLine.Space(),
            ReportLine.Section("Informations du vehicule"),
            ReportLine.Text($"Type: {declaration.VehicleType}"),
            ReportLine.Text($"Marque: {declaration.VehicleBrand}"),
            ReportLine.Text($"Numero chassis: {declaration.ChassisNumber}"),
            ReportLine.Space(),
            ReportLine.Section("Informations de l'acheteur"),
            ReportLine.Text($"Nom de l'acheteur: {declaration.BuyerName}"),
            ReportLine.Text($"Adresse de l'acheteur: {declaration.BuyerAddress}"),
            ReportLine.Text($"CIN de l'acheteur: {declaration.BuyerCin}"),
            ReportLine.Text($"Telephone de l'acheteur: {declaration.BuyerPhone}"),
            ReportLine.Space(),
            ReportLine.Section("Documents et observation")
        };

        lines.AddRange(Wrap($"Observation: {declaration.Observation ?? "Aucune"}", 92).Select(ReportLine.Text));

        if (declaration.Attachments.Count == 0)
        {
            lines.Add(ReportLine.Text("Pieces jointes: Aucune"));
        }
        else
        {
            lines.Add(ReportLine.Text("Pieces jointes:"));

            foreach (var attachment in declaration.Attachments)
            {
                lines.AddRange(Wrap($"- {attachment.OriginalFileName} ({FormatFileSize(attachment.SizeBytes)})", 92).Select(ReportLine.Text));
            }
        }

        lines.AddRange(
        [
            ReportLine.Space(),
            ReportLine.Section("Signatures"),
            ReportLine.Text($"Signature du vendeur: {declaration.SellerSignature}"),
            ReportLine.Text($"Signature du gerant: {declaration.ManagerSignature}"),
            ReportLine.Text($"Signature de l'acheteur: {declaration.BuyerSignature}"),
            ReportLine.Space(),
            ReportLine.Text("Document imprime depuis le registre des declarations de vente.")
        ]);

        return lines;
    }

    private static byte[] BuildPdf(IReadOnlyList<IReadOnlyList<ReportLine>> pages)
    {
        var objectCount = 4 + pages.Count * 2;
        var fontObject = objectCount - 1;
        var boldFontObject = objectCount;
        var offsets = new List<long> { 0 };
        using var stream = new MemoryStream();

        WriteAscii(stream, "%PDF-1.4\n");
        WriteObject(stream, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");

        var kids = string.Join(" ", Enumerable.Range(0, pages.Count).Select(index => $"{3 + index * 2} 0 R"));
        WriteObject(stream, offsets, 2, $"<< /Type /Pages /Kids [{kids}] /Count {pages.Count} >>");

        for (var index = 0; index < pages.Count; index++)
        {
            var pageObject = 3 + index * 2;
            var contentObject = pageObject + 1;
            var page = $"""
                << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 {fontObject} 0 R /F2 {boldFontObject} 0 R >> >> /Contents {contentObject} 0 R >>
                """;
            WriteObject(stream, offsets, pageObject, page.Trim());

            var content = BuildPageContent(pages[index], index + 1, pages.Count);
            WriteStreamObject(stream, offsets, contentObject, content);
        }

        WriteObject(stream, offsets, fontObject, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        WriteObject(stream, offsets, boldFontObject, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

        var xrefOffset = stream.Position;
        WriteAscii(stream, $"xref\n0 {objectCount + 1}\n");
        WriteAscii(stream, "0000000000 65535 f \n");

        foreach (var offset in offsets.Skip(1))
        {
            WriteAscii(stream, $"{offset:0000000000} 00000 n \n");
        }

        WriteAscii(stream, $"trailer\n<< /Size {objectCount + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        return stream.ToArray();
    }

    private static string BuildPageContent(IReadOnlyList<ReportLine> lines, int pageNumber, int totalPages)
    {
        var builder = new StringBuilder();
        var y = 750;

        foreach (var line in lines)
        {
            if (line.Kind == ReportLineKind.Space)
            {
                y -= 12;
                continue;
            }

            var font = line.Kind == ReportLineKind.Text ? "F1" : "F2";
            var size = line.Kind == ReportLineKind.Title ? 18 : line.Kind == ReportLineKind.Section ? 13 : 10;
            var x = line.Kind == ReportLineKind.Title ? 50 : line.Kind == ReportLineKind.Section ? 50 : 62;
            builder.Append(CultureInvariant($"BT /{font} {size} Tf {x} {y} Td ({Escape(line.Value)}) Tj ET\n"));
            y -= line.Kind == ReportLineKind.Title ? 28 : 17;
        }

        builder.Append(CultureInvariant($"BT /F1 9 Tf 50 32 Td (Page {pageNumber} / {totalPages}) Tj ET\n"));
        return builder.ToString();
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
                _ when character <= 255 => character.ToString(),
                _ => "?"
            });
        }

        return builder.ToString();
    }

    private static IEnumerable<string> Wrap(string value, int length)
    {
        if (value.Length <= length)
        {
            yield return value;
            yield break;
        }

        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var line = new StringBuilder();

        foreach (var word in words)
        {
            if (line.Length + word.Length + 1 > length)
            {
                yield return line.ToString();
                line.Clear();
            }

            if (line.Length > 0)
            {
                line.Append(' ');
            }

            line.Append(word);
        }

        if (line.Length > 0)
        {
            yield return line.ToString();
        }
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

    private sealed record ReportLine(ReportLineKind Kind, string Value)
    {
        public static ReportLine Title(string value) => new(ReportLineKind.Title, value);

        public static ReportLine Section(string value) => new(ReportLineKind.Section, value);

        public static ReportLine Text(string value) => new(ReportLineKind.Text, value);

        public static ReportLine Space() => new(ReportLineKind.Space, string.Empty);
    }

    private enum ReportLineKind
    {
        Title,
        Section,
        Text,
        Space
    }
}
