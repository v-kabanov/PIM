// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-01
// Comment  
// **********************************************************************************************/
// 
using System.IO;

namespace FulltextStorageLib
{
    /// <summary>
    ///     Specialized indexed document storage for notes.
    /// </summary>
    public interface INoteStorage : IDocumentStorageWithFulltextSearch<Note, string, INoteHeader>
    {
        bool UpdateLastUpdateTimeAutomatically { get; set; }

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

        public bool UpdateLastUpdateTimeAutomatically
        {
            get { return }
            set;
        }
        public string RootPath { get; private set; }

        /// <summary>
        ///     Factory method creating standard indexed note storage.
        /// </summary>
        /// <param name="rootDirectoryPath">
        ///     Path to the root directory containing all storage and index files.
        /// </param>
        /// <returns></returns>
        public static NoteStorage CreateStandard(string rootDirectoryPath)
        {
            var dbPath = Path.Combine(rootDirectoryPath, "db");
            var fulltextPath = Path.Combine(rootDirectoryPath, "ft");

            Directory.CreateDirectory(dbPath);
            Directory.CreateDirectory(fulltextPath);

            var luceneAdapter = new LuceneNoteAdapter();

            var storage = new CouchbaseStorage<Note>(rootDirectoryPath, new NoteCouchbaseAdapter());
            var multiIndex = new MultiIndex(luceneAdapter.DocumentKeyName);
            var searchEngine = new SearchEngine<Note, INoteHeader, string>(fulltextPath, luceneAdapter, multiIndex);

            var result = new NoteStorage(storage, searchEngine) {RootPath = rootDirectoryPath};

            return result;
        }
    }
}