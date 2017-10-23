// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-15
// Comment  
// **********************************************************************************************/
// 
using System;
using System.Collections.Generic;
using FulltextStorageLib.Util;
using Pim.CommonLib;

namespace FulltextStorageLib
{
    public class NoteAdapter : IDocumentAdapter<Note>
    {
        public bool SetLastUpdateTimeWithVersionIncrement { get; set; }

        public NoteAdapter(bool setLastUpdateTimeWithVersionIncrement)
        {
            SetLastUpdateTimeWithVersionIncrement = setLastUpdateTimeWithVersionIncrement;
        }

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
            if (SetLastUpdateTimeWithVersionIncrement)
                document.LastUpdateTime = DateTime.Now;
            return ++document.Version;
        }
    }
}