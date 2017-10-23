// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-01
// Comment  
// **********************************************************************************************/
// 
using System.IO;
using FulltextStorageLib.Util;
using LiteDB;
using Pim.CommonLib;

namespace FulltextStorageLib
{
    /// <summary>
    ///     Specialized indexed document storage for notes.
    /// </summary>
    public interface INoteStorage : IDocumentStorageWithFulltextSearch<Note, string, INoteHeader>
    {
        /// <summary>
        ///     Root directory path.
        /// </summary>
        string RootPath { get; }
    }

    /// <summary>
    ///     Specialized indexed document storage for notes.
    /// </summary>
    public class NoteStorage : DocumentStorageWithFulltextSearch<Note, string, INoteHeader>, INoteStorage
    {
        public NoteStorage(IDocumentStorage<Note, string> storage, IStandaloneFulltextSearchEngine<Note, INoteHeader, string> searchEngine)
            : base(storage, searchEngine)
        {
        }

        /// <summary>
        ///     Optional, may be null if storage does not have single root directory.
        /// </summary>
        public string RootPath { get; private set; }

        /// <summary>
        ///     Factory method creating standard indexed note storage.
        /// </summary>
        /// <param name="rootDirectoryPath">
        ///     Path to the root directory containing all storage and index files.
        /// </param>
        /// <param name="updateLastUpdateAutomatically">
        ///     Set <see cref="IFulltextIndexEntry.LastUpdateTime"/> to current time when saving. If false, last update time is maintained by the client.
        /// </param>
        /// <returns></returns>
        public static NoteStorage CreateStandard(string rootDirectoryPath, bool updateLastUpdateAutomatically = false)
        {
            var dbPath = Path.Combine(rootDirectoryPath, "Pim.db");
            var database = NoteLiteDb.GetNoteDatabase($"Filename={dbPath}; Upgrade=true; Initial Size=5MB; Password=;");
            return CreateStandard(database, rootDirectoryPath, updateLastUpdateAutomatically);
        }

        public static NoteStorage CreateStandard(LiteDatabase database, string rootDirectoryPath, bool updateLastUpdateAutomatically = false)
        {
            Check.DoRequireArgumentNotNull(database, nameof(database));
            Check.DoRequireArgumentNotNull(rootDirectoryPath, nameof(rootDirectoryPath));

            var fulltextPath = Path.Combine(rootDirectoryPath, "ft");
            Directory.CreateDirectory(fulltextPath);


            var luceneAdapter = new LuceneNoteAdapter();
            var storage = new LiteDbStorage<Note>(database, new NoteAdapter(updateLastUpdateAutomatically));
            var multiIndex = new MultiIndex(luceneAdapter.DocumentKeyName);
            var searchEngine = new SearchEngine<Note, INoteHeader, string>(fulltextPath, luceneAdapter, multiIndex);

            var result = new NoteStorage(storage, searchEngine) { RootPath = rootDirectoryPath };

            return result;
        }

        /// <summary>
        ///     Factory method creating standard indexed note storage.
        /// </summary>
        /// <param name="rootDirectoryPath">
        ///     Path to the root directory containing all storage and index files.
        /// </param>
        /// <param name="updateLastUpdateAutomatically">
        ///     Set <see cref="IFulltextIndexEntry.LastUpdateTime"/> to current time when saving. If false, last update time is maintained by the client.
        /// </param>
        /// <returns></returns>
        public static NoteStorage CreateCouchbase(string rootDirectoryPath, bool updateLastUpdateAutomatically = false)
        {
            var dbPath = Path.Combine(rootDirectoryPath, "db");
            var fulltextPath = Path.Combine(rootDirectoryPath, "ft");

            Directory.CreateDirectory(dbPath);
            Directory.CreateDirectory(fulltextPath);

            var luceneAdapter = new LuceneNoteAdapter();

            var storage = new CouchbaseStorage<Note>(dbPath, new NoteCouchbaseAdapter(updateLastUpdateAutomatically));
            var multiIndex = new MultiIndex(luceneAdapter.DocumentKeyName);
            var searchEngine = new SearchEngine<Note, INoteHeader, string>(fulltextPath, luceneAdapter, multiIndex);

            var result = new NoteStorage(storage, searchEngine) { RootPath = rootDirectoryPath };

            return result;
        }
    }
}