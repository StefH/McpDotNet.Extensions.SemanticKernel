using System.Text;
using DocumentFormat.OpenXml.Packaging;
using Stef.Validation;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace ModelContextProtocolServer.OpenXml.Stdio;

public static class WordDocumentReader
{
    public static string GetTextFromWordDocument(string filePath)
    {
        try
        {
            using var doc = WordprocessingDocument.Open(filePath, false);

            if (doc.MainDocumentPart == null)
            {
                return string.Empty;
            }

            var body = ReadBody(doc.MainDocumentPart);
            if (body == null)
            {
                return string.Empty;
            }

            var textBuilder = new StringBuilder();

            // Get all paragraphs in the document
            var paragraphs = body.Elements<Paragraph>();
            foreach (var element in paragraphs)
            {
                textBuilder.AppendLine(GetTextFromElement(element));
                textBuilder.AppendLine();
            }

            textBuilder.AppendLine();

            // Get all tables in the document
            var tables = body.Elements<Table>();
            foreach (var element in tables)
            {
                textBuilder.AppendLine(GetTextFromElement(element));
                textBuilder.AppendLine();
            }

            return textBuilder.ToString().Trim();
        }
        catch (Exception ex)
        {
            // Handle any exceptions here
            Console.WriteLine($"Error reading document: {ex.Message}");
            return string.Empty;
        }
    }

    private static Body? ReadBody(MainDocumentPart main)
    {
        // For some reason, for some documents, we need to read twice.
        try
        {
            return main.Document.Body;
        }
        catch
        {
            return main.Document.Body;
        }
    }

    private static string GetTextFromElement(OpenXmlCompositeElement? element)
    {
        if (element == null)
        {
            return string.Empty;
        }

        // Get the text from all runs in the element
        return string.Join("", element.Descendants<Text>().Select(t => t.Text));
    }
}