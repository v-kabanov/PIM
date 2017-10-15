// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-27
// Comment  
// **********************************************************************************************/
// 

using System;
using Couchbase.Lite;
using FulltextStorageLib.Util;

namespace FulltextStorageLib
{
    public class NoteCouchbaseAdapter : NoteAdapter, ICouchbaseDocumentAdapter<Note>
    {

        public NoteCouchbaseAdapter(bool setLastUpdateTimeWithVersionIncrement)
            : base (setLastUpdateTimeWithVersionIncrement)
        {
        }

        public Note Read(Document document)
        {
            Check.DoRequireArgumentNotNull(document, nameof(document));

            return new Note
            {
                Id = document.Id,
                CreateTime = document.GetProperty<DateTime>(LuceneNoteAdapter.FieldNameCreateTime),
                Text = document.GetProperty<string>(LuceneNoteAdapter.FieldNameText),
                LastUpdateTime = document.GetProperty<DateTime>(LuceneNoteAdapter.FieldNameLastUpdateTime),
                Version = document.GetProperty<int>(LuceneNoteAdapter.FieldNameVersion)
            };
        }
    }
}