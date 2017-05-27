// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-03
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using AuNoteLib.Util;
using Couchbase.Lite;

namespace AuNoteLib
{
    /// <summary>
    ///     Couchbase Lite based storage for notes.
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class CouchbaseStorage<TDoc> : IDocumentStorage<TDoc, string>
        where TDoc : class
    {
        private const string AppName = "mynotes";

        private Manager Manager { get; }

        private Database Database { get; }

        public ICouchbaseDocumentAdapter<TDoc> DocumentAdapter { get; }

        /// <param name="path">
        ///     Mandatory, root storage directory path. Created if it does not exist.
        /// </param>
        /// <param name="documentAdapter">
        ///     Mandatory
        /// </param>
        public CouchbaseStorage(string path, ICouchbaseDocumentAdapter<TDoc> documentAdapter)
        {
            Check.DoRequireArgumentNotNull(path, nameof(path));
            Check.DoRequireArgumentNotNull(documentAdapter, nameof(documentAdapter));

            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }

            DocumentAdapter = documentAdapter;

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
        public void SaveOrUpdate(TDoc document)
        {
            SaveOrUpdateImpl(document);
        }

        public void SaveOrUpdate(params TDoc[] docs)
        {
            foreach(var document in docs)
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
        public TDoc GetExisting(string id)
        {
            var doc = Database.GetExistingDocument(id);

            if (doc == null)
                return null;

            return DocumentAdapter.Read(doc);
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
        public TDoc Delete(string id)
        {
            var doc = Database.GetExistingDocument(id);

            if (doc == null)
                return null;

            doc.Delete();

            return DocumentAdapter.Read(doc);
        }

        /// <summary>
        ///     Get lazily loaded collection of all notes in the database.
        /// </summary>
        /// <returns>
        ///     Lazily loaded collection; safe to invoke on large databases.
        /// </returns>
        public IEnumerable<TDoc> GetAll()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var row in Database.CreateAllDocumentsQuery().Run())
                yield return DocumentAdapter.Read(row.Document);
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
        /// <param name="document">
        ///     not nullable
        /// </param>
        /// <returns>
        ///     null if there is no change;
        /// </returns>
        private SavedRevision SaveOrUpdateImpl(TDoc document)
        {
            Check.DoRequireArgumentNotNull(document, nameof(document));

            Document doc;

            var save = DocumentAdapter.IsTransient(document);

            if (save)
            {
                doc = Database.CreateDocument();
            }
            else
            {
                var id = DocumentAdapter.GetId(document);
                doc = Database.GetExistingDocument(id);
                save = doc == null;

                if (doc != null)
                {
                    var existingNote = DocumentAdapter.Read(doc);
                    save = DocumentAdapter.IsChanged(existingNote, document);
                }
                else
                {
                    doc = Database.CreateDocument();
                    doc.Id = id;
                }
            }

            SavedRevision result = null;

            if (save)
            {
                DocumentAdapter.IncrementVersion(document);
                result = doc.PutProperties(DocumentAdapter.ToDictionary(document));
            }

            return result;
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