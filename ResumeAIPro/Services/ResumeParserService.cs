using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ResumeAIPro.Models;

// iText7 for PDF
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

// OpenXML for DOCX
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ResumeAIPro.Services
{
    public class ResumeParserService
    {
        // ── Public Entry Point ──────────────────────────────────
        public Task<ResumeData> ParseAsync(string filePath)
        {
            return Task.Run(() =>
            {
                var ext = Path.GetExtension(filePath).ToLowerInvariant();
                string rawText = ext switch
                {
                    ".pdf"  => ExtractFromPdf(filePath),
                    ".docx" => ExtractFromDocx(filePath),
                    ".doc"  => throw new NotSupportedException("Old .doc format is not supported. Please save as .docx."),
                    ".txt"  => File.ReadAllText(filePath),
                    _       => throw new NotSupportedException($"File type '{ext}' is not supported.")
                };

                return new ResumeData
                {
                    FilePath  = filePath,
                    FileName  = Path.GetFileName(filePath),
                    RawText   = rawText,
                    // Quick regex extractions for preview (AI will do full extraction)
                    Email     = ExtractEmail(rawText),
                    Phone     = ExtractPhone(rawText),
                    LinkedInUrl = ExtractLinkedIn(rawText)
                };
            });
        }

        // ── PDF Extraction ──────────────────────────────────────
        private static string ExtractFromPdf(string path)
        {
            var sb = new StringBuilder();
            using var reader = new PdfReader(path);
            using var pdf    = new PdfDocument(reader);

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var strategy = new SimpleTextExtractionStrategy();
                var pageText  = PdfTextExtractor.GetTextFromPage(pdf.GetPage(i), strategy);
                sb.AppendLine(pageText);
            }
            return sb.ToString().Trim();
        }

        // ── DOCX Extraction ────────────────────────────────────
        private static string ExtractFromDocx(string path)
        {
            var sb = new StringBuilder();
            using var doc = WordprocessingDocument.Open(path, false);
            var body = doc.MainDocumentPart?.Document.Body;
            if (body is null) return "";

            foreach (var para in body.Descendants<Paragraph>())
            {
                sb.AppendLine(para.InnerText);
            }
            return sb.ToString().Trim();
        }

        // ── Quick Regex Extractions ────────────────────────────
        private static string ExtractEmail(string text)
        {
            var match = Regex.Match(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            return match.Success ? match.Value : "";
        }

        private static string ExtractPhone(string text)
        {
            var match = Regex.Match(text,
                @"(\+?\d{1,3}[-.\s]?)?(\(?\d{3}\)?[-.\s]?)?\d{3}[-.\s]?\d{4}");
            return match.Success ? match.Value.Trim() : "";
        }

        private static string ExtractLinkedIn(string text)
        {
            var match = Regex.Match(text,
                @"linkedin\.com/in/[a-zA-Z0-9\-]+", RegexOptions.IgnoreCase);
            return match.Success ? "https://www." + match.Value : "";
        }
    }
}
