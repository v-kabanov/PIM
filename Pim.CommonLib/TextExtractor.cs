using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;

namespace Pim.CommonLib;

public interface ITextExtractor
{
    string Extract(string filePath);
    
    string Extract(string fileName, Stream content);
}

public class TextExtractor : ITextExtractor
{
    private const string PdfFileNameExtension = ".pdf";
    private const string DocxFileNameExtension = ".docx";
    private static readonly string[] DefaultTextFileExtensions = {".txt", ".log", ".md", ".xml", ".sql", ".cs", ".htm", ".html", ".text", ".py"};
    
    private readonly Encoding _utfEncoding = new UTF8Encoding(false, false);
    // {
    //     EncoderFallback = new EncoderReplacementFallback(" "),
    //     DecoderFallback = new DecoderReplacementFallback(" ")
    // };
    
    private string[] TextFileExtensions { get; }

    public TextExtractor(string[] textFileExtensions = null)
    {
        TextFileExtensions = textFileExtensions == null || textFileExtensions.Length == 0
            ? DefaultTextFileExtensions
            : textFileExtensions;
    }

    public string Extract(string filePath)
    {
        var textCollector = new StringBuilder(64 * 1024);
        if (Extract(filePath, textCollector))
            return textCollector.ToString();
        
        return null;
    }

    public string Extract(string fileName, Stream content)
    {
        var textCollector = new StringBuilder(64 * 1024);
        if (Extract(fileName, content, textCollector))
            return textCollector.ToString();
        
        return null;
    }
    
    public bool Extract(string filePath, StringBuilder textCollector)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        if (textCollector == null) throw new ArgumentNullException(nameof(textCollector));
        
        var result = false;
        
        var extension = Path.GetExtension(filePath);
        
        if (PdfFileNameExtension.EqualsIgnoreCase(extension))
        {
            using var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly);
            result = ExtractText(doc, textCollector);
        }
        else if (DocxFileNameExtension.EqualsIgnoreCase(extension))
            result = ExtractFromDocx(filePath, textCollector);
        else if (extension.InIgnoreCase(TextFileExtensions))
        {
            var text = File.ReadAllText(filePath).Trim();
            result = AppendLine(textCollector, text);
        }
        
        return result;
    }
    
    public bool Extract(string fileName, Stream content, StringBuilder textCollector)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));

        var result = false;
        
        var extension = Path.GetExtension(fileName);
        
        if (PdfFileNameExtension.EqualsIgnoreCase(extension))
        {
            using var doc = PdfReader.Open(content, PdfDocumentOpenMode.ReadOnly);
            result = ExtractText(doc, textCollector);
        }
        else if (DocxFileNameExtension.EqualsIgnoreCase(extension))
            result = ExtractFromDocx(content, textCollector);
        else if (extension.InIgnoreCase(TextFileExtensions))
        {
            var text = new StreamReader(content).ReadToEnd().Trim();
            result = AppendLine(textCollector, text);
        }
        
        return result;
    }
    
    private bool ExtractText(PdfDocument document, StringBuilder textCollector)
    {
        var result = false;

        foreach (var page in document.Pages)
        {
            var sequence = ContentReader.ReadContent(page);
            result |= ExtractText(sequence, textCollector);
        }
        
        return result;
    }
    
    
    private bool ExtractText(CObject obj, StringBuilder textCollector)
    {
        var result = false;
        
        if (obj is CComment comment)
            result |= AppendLine(textCollector, comment.Text);
        // number objects appear inside words
        // else if (obj is CNumber)
        // {
        //     textCollector.Append(obj).Append(' ');
        //     result = true;
        // }
        else if (obj is COperator @operator)
        {
            if (@operator.OpCode.OpCodeName is OpCodeName.Tj or OpCodeName.TJ)
                result |= ExtractText(@operator.Operands, textCollector);
        }
        else if (obj is CSequence sequence)
            result |= ExtractText(sequence, textCollector);
        else if (obj is CString ostring)
            result |= Append(textCollector, ostring.Value);
        else
            Console.WriteLine("Ignoring PDF object of type {0}", obj.GetType().Name);
        
        return result;
    }

    private string Sanitize(string value)
    {
        if (value.IsNullOrWhiteSpace())
            return value;
        
        try
        {
            if (value.EnumerateRunes().Any(x => Rune.GetUnicodeCategory(x) >= UnicodeCategory.Control))
            {
                var buf = new StringBuilder(value.Length);
                
                foreach (var rune in value.EnumerateRunes())
                {
                    var cat = Rune.GetUnicodeCategory(rune);
                    if (cat < UnicodeCategory.Control)
                        buf.Append(rune.ToString());
                    else if (buf.Length == 0 || buf[buf.Length - 1] != ' ')
                        buf.Append(' ');
                }
                value = buf.ToString();
            }
            return _utfEncoding.GetString(_utfEncoding.GetBytes(value));
        }
        catch
        {
            // should not occur
            return null;
        }
    }
    
    private bool AppendLine(StringBuilder bld, string value)
    {
        var result = Append(bld, value);
        if (result)
            bld.AppendLine();
        return result;
    }
    
    private bool Append(StringBuilder bld, string value)
    {
        if (value.IsNullOrEmpty())
            return false;
        
        value = Sanitize(value);
        if (value.IsNullOrEmpty())
            return false;
        
        bld.Append(value);
        return true;
    }

    private bool ExtractFromDocx(string fullPath, StringBuilder textCollector)
    {
        using var document = WordprocessingDocument.Open(fullPath, false);
        
        return ExtractFromDocx(document, textCollector);
    }

    private bool ExtractFromDocx(Stream content, StringBuilder textCollector)
    {
        using var document = WordprocessingDocument.Open(content, false);
        
        return ExtractFromDocx(document, textCollector);
    }
    
    private bool ExtractFromDocx(WordprocessingDocument document, StringBuilder textCollector)
    {
        var result = false;
        
        var nameTable = new NameTable();
        var xmlNamespaceManager = new XmlNamespaceManager(nameTable);
        xmlNamespaceManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

        string wordprocessingDocumentText;
        using(var streamReader = new StreamReader(document.MainDocumentPart.GetStream()))
            wordprocessingDocumentText = streamReader.ReadToEnd();

        var xmlDocument = new XmlDocument(nameTable);
        xmlDocument.LoadXml(wordprocessingDocumentText);

        var paragraphNodes = xmlDocument.SelectNodes("//w:p", xmlNamespaceManager);
        foreach(XmlNode paragraphNode in paragraphNodes)
        {
            var textNodes = paragraphNode.SelectNodes(".//w:t | .//w:tab | .//w:br", xmlNamespaceManager);
            foreach(XmlNode textNode in textNodes)
            {
                switch(textNode.Name)
                {
                    case "w:t":
                        result |= Append(textCollector, textNode.InnerText);
                        break;

                    case "w:tab":
                        if (result)
                            textCollector.Append('\t');
                        break;

                    case "w:br":
                        if (result)
                            textCollector.Append('\v');
                        break;
                }
            }

            textCollector.AppendLine();
        }
        return result;
    }

    private bool ExtractText(CSequence obj, StringBuilder textCollector)
    {
        var result = false;
        
        foreach (var element in obj)
            result |= ExtractText(element, textCollector);
        
        return result;
    }
}