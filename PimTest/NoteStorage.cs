// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-03
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using Couchbase.Lite;
using NFluent;

namespace PimTest
{
    public class NoteStorage : IDisposable
    {
        public const string AppName = "mynotes";

        protected Manager Manager { get; private set; }
        protected Database Database { get; private set; }

        public NoteStorage(string path)
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }
            Manager = new Manager(di, ManagerOptions.Default);
            var options = new DatabaseOptions() { Create = true };

            Database = Manager.GetDatabase(AppName);
        }

        /// <summary>
        ///     If note is transient (<see cref="INote.IsTransient"/>), generates and assigns new Id and saves.<br/>
        ///     Saves if:<br/>
        ///         - note is transient<br/>
        ///         - note is not <see cref="INote.IsTransient"/>, but does not exist in the database yet; uses its <see cref="INoteHeader.Id"/><br/>
        ///     Updates is not transient, exists in db and <see cref="INote.Text"/> is different from what is in the db.
        /// </summary>
        /// <param name="note">
        ///     not nullable
        /// </param>
        /// <returns>
        ///     null if there is no change;
        /// </returns>
        public SavedRevision SaveOrUpdate(Note note)
        {
            Document doc;

            bool save = note.IsTransient;

            if (note.IsTransient)
            {
                doc = Database.CreateDocument();
                note.Id = doc.Id;
            }
            else
            {
                doc = Database.GetExistingDocument(note.Id);
                save = doc == null;

                if (doc != null)
                {
                    var existingNote = ToNote(doc);
                    save = existingNote.Text != note.Text;
                }
                else
                {
                    doc = new Document(Database, note.Id);
                }
            }

            SavedRevision result = null;

            if (save)
                result = doc.PutProperties(ToDictionary(note));

            return result;
        }

        /// <summary>
        ///     Returns null if not found; exception thrown if found, but conversion failed.
        /// </summary>
        /// <param name="id">
        /// </param>
        /// <returns>
        ///     null if not found
        /// </returns>
        public Note GetExistingNote(string id)
        {
            var doc = Database.GetExistingDocument(id);

            if (doc == null)
                return null;

            return ToNote(doc);
        }

        public Note DeleteNote(string id)
        {
            var doc = Database.GetExistingDocument(id);

            doc.Delete();

            return doc != null ? ToNote(doc) : null;
        }

        private Note ToNote(Document doc)
        {
            return new Note()
            {
                Id = doc.Id,
                CreateTime = doc.GetProperty<DateTime>(LuceneNoteAdapter.FieldNameCreateTime),
                LastUpdateTime = doc.GetProperty<DateTime>(LuceneNoteAdapter.FieldNameLastUpdateTime),
                Text = doc.GetProperty<string>(LuceneNoteAdapter.FieldNameText),
            };
        }

        private Dictionary<string, object> ToDictionary(Note note)
        {
            Check.That(note).IsNotNull();

            return new Dictionary<string, object>()
            {
                { LuceneNoteAdapter.FieldNameCreateTime, note.CreateTime }
                , { LuceneNoteAdapter.FieldNameText, note.Text }
                , { LuceneNoteAdapter.FieldNameLastUpdateTime, note.LastUpdateTime }
            };
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (Database != null)
                        Database.Dispose();

                    if (Manager != null)
                        Manager.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NoteStorage() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}