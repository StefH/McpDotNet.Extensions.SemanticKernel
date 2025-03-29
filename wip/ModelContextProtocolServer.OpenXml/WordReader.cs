using DocumentFormat.OpenXml.Packaging;

namespace ModelContextProtocolServer.OpenXml;

public static class WordReader
{
    public static string ReadWordDocument(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        using var wordDoc = WordprocessingDocument.Open(filePath, false);

        var body = wordDoc.MainDocumentPart?.Document.Body;
        return body?.InnerText ?? string.Empty;
    }
}