// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/

using Lucene.Net.Documents;

namespace FulltextStorageLib;

public class LuceneSearchHit
{
    public LuceneSearchHit(Document document, float score, string keyFieldName)
    {
        KeyFieldName = keyFieldName;
        Document = document;
        Score = score;
    }

    /// <summary>
    ///     Name of the field for eg <see cref="Lucene.Net.Documents.Lucene.Net.Documents.Document.Get(string)"/> to retrieve entity key.
    /// </summary>
    public string KeyFieldName { get; private set; }

    public Document Document { get; private set; }

    public float Score { get; private set; }

    public string EntityId => Document.Get(KeyFieldName);
}