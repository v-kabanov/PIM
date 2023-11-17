using System;
using System.Text;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;

namespace Pim.CommonLib;

public static class TextExtractor
{
    public static void Extract(string filePath, StringBuilder textCollector)
    {
        using var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly);
        foreach (var page in doc.Pages)
        {
            var sequence = ContentReader.ReadContent(page);
            ExtractText(sequence, textCollector);
        }
    }
    
    private static void ExtractText(CObject obj, StringBuilder target)
    {
        if (obj is CComment comment)
            target.AppendLine(comment.Text);
        else if (obj is CNumber)
            target.Append(obj).Append(" ");
        else if (obj is COperator @operator)
        {
            if (@operator.OpCode.OpCodeName is OpCodeName.Tj or OpCodeName.TJ)
                ExtractText(@operator.Operands, target);
        }
        else if (obj is CSequence sequence)
            ExtractText(sequence, target);
        else if (obj is CString ostring)
            target.Append(ostring.Value);
        else
        {
            Console.WriteLine("Ignoring PDF object of type {0}", obj.GetType().Name);
        }
    }

    private static void ExtractText(CSequence obj, StringBuilder target)
    {
        foreach (var element in obj)
        {
            ExtractText(element, target);
        }
    }
}