// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-27
// Comment  
// **********************************************************************************************/
// 

using System;
using System.Collections.Generic;
using Couchbase.Lite;
using FulltextStorageLib.Util;

namespace FulltextStorageLib
{
    public class NoteCouchbaseAdapter : ICouchbaseDocumentAdapter<Note>
    {
        public string GetId(Note document)
        {
            Check.DoRequireArgumentNotNull(document, nameof(document));

            return document.Id;
        }

        public IDictionary<string, object> ToDictionary(Note document)
        {
            Check.DoRequireArgumentNotNull(document, nameof(document));

            return new Dictionary<string, object>
            {
                { LuceneNoteAdapter.FieldNameCreateTime, document.CreateTime }
                , { LuceneNoteAdapter.FieldNameText, document.Text }
                , { LuceneNoteAdapter.FieldNameLastUpdateTime, document.LastUpdateTime }
                , { LuceneNoteAdapter.FieldNameVersion, document.Version }
            };
        }

        public Note Read(Document document)
        {
            Check.DoRequireArgumentNotNull(document, nameof(document));

            return new Note
            {
                Id = document.Id,
                CreateTime = document.GetProperty<DateTime>(LuceneNoteAdapter.FieldNameCreateTime),
                LastUpdateTime = document.GetProperty<DateTime>(LuceneNoteAdapter.FieldNameLastUpdateTime),
                Text = document.GetProperty<string>(LuceneNoteAdapter.FieldNameText),
                Version = document.GetProperty<int>(LuceneNoteAdapter.FieldNameVersion)
            };
        }

        public bool IsTransient(Note document)
        {
            Check.DoRequireArgumentNotNull(document, nameof(document));

            return document.IsTransient;
        }

        public bool IsChanged(Note version1, Note version2)
        {
            Check.DoRequireArgumentNotNull(version1, nameof(version1));
            Check.DoRequireArgumentNotNull(version2, nameof(version2));

            return version1.Text != version2.Text;
        }

        public int IncrementVersion(Note document)
        {
            Check.DoRequireArgumentNotNull(document.Id, nameof(document.Id));

            return ++document.Version;
        }
    }
}