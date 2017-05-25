// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-03
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Couchbase.Lite;
using NFluent;

namespace AuNoteLib
{
    public interface INoteStorage : IDocumentStorage<Note, string>
    {
    }

    /// <summary>
    ///     Couchbase Lite based storage for notes.
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class NoteStorage : INoteStorage
    {
        private const string AppName = "mynotes";

        private Manager Manager { get; }

        private Database Database { get; }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="path"></param>
        public NoteStorage(string path)
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }

            Manager = new Manager(di, ManagerOptions.Default);

            Database = Manager.GetDatabase(AppName);
        }

        /// <summary>
        ///     If note is transient (<see cref="INote.IsTransient"/>), generates and assigns new Id and saves.<br/>
        ///     Saves if:<br/>
        ///         - note is transient<br/>
        ///         - note is not <see cref="INote.IsTransient"/>, but does not exist in the database yet; uses its <see cref="INoteHeader.Id"/><br/>
        ///     Updates is not transient, exists in db and <see cref="INote.Text"/> is different from what is in the db.
        /// </summary>
        /// <param name="document">
        ///     Mandatory
        /// </param>
        public void SaveOrUpdate(Note document)
        {
            SaveOrUpdateImpl(document);
        }

        /// <summary>
        ///     Returns null if not found; exception thrown if found, but conversion failed.
        /// </summary>
        /// <param name="id">
        /// </param>
        /// <returns>
        ///     null if not found
        /// </returns>
        public Note GetExisting(string id)
        {
            var doc = Database.GetExistingDocument(id);

            if (doc == null)
                return null;

            return ToNote(doc);
        }

        /// <summary>
        ///     Remove from storage by primary key.
        /// </summary>
        /// <param name="id">
        ///     Document key
        /// </param>
        /// <returns>
        ///     Null if not found
        /// </returns>
        public Note Delete(string id)
        {
            var doc = Database.GetExistingDocument(id);

            if (doc == null)
                return null;

            doc.Delete();

            return ToNote(doc);
        }

        /// <summary>
        ///     Get lazily loaded collection of all notes in the database.
        /// </summary>
        /// <returns>
        ///     Lazily loaded collection; safe to invoke on large databases.
        /// </returns>
        public IEnumerable<Note> GetAll()
        {
            foreach(var row in Database.CreateAllDocumentsQuery().Run())
            {
                yield return ToNote(row.Document);
            }
        }

        /// <summary>
        ///     Get total number of documents
        /// </summary>
        /// <returns></returns>
        public int CountAll()
        {
            return Database.GetDocumentCount();
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
        public SavedRevision SaveOrUpdateImpl(IPersistentNote note)
        {
            Check.That(note).IsNotNull();
            Check.That(note.Id).IsNotEmpty();
            Check.That(note.Name).IsNotEmpty();

            Document doc;

            var save = note.IsTransient;

            if (note.IsTransient)
            {
                doc = new Document(Database, note.Id);
                // note.Id = doc.Id; // let db assign id
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
            {
                note.IncrementVersion();
                result = doc.PutProperties(ToDictionary(note));
            }

            return result;
        }

        private Note ToNote(Document doc)
        {
            return new Note
            {
                Id = doc.Id,
                CreateTime = doc.GetProperty<DateTime>(LuceneNoteAdapter.FieldNameCreateTime),
                LastUpdateTime = doc.GetProperty<DateTime>(LuceneNoteAdapter.FieldNameLastUpdateTime),
                Text = doc.GetProperty<string>(LuceneNoteAdapter.FieldNameText),
                Version = doc.GetProperty<int>(LuceneNoteAdapter.FieldNameVersion)
            };
        }

        private Dictionary<string, object> ToDictionary(INote note)
        {
            Check.That(note).IsNotNull();

            return new Dictionary<string, object>
            {
                { LuceneNoteAdapter.FieldNameCreateTime, note.CreateTime }
                , { LuceneNoteAdapter.FieldNameText, note.Text }
                , { LuceneNoteAdapter.FieldNameLastUpdateTime, note.LastUpdateTime }
                , { LuceneNoteAdapter.FieldNameVersion, note.Version }
            };
        }

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    Database?.Dispose();

                    Manager?.Close();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NoteStorage() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }
}