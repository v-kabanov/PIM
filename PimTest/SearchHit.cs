// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-30
// Comment		
// **********************************************************************************************/

using Lucene.Net.Documents;

namespace PimTest
{
    public class SearchHit
    {
        public SearchHit(Document document, float score)
        {
            Document = document;
            Score = score;
        }

        public Document Document { get; private set; }
        public float Score { get; private set; }

        public string NoteId
        {
            get { return Document.Get(LuceneNoteAdapter.FieldNameId); }
        }
    }
}