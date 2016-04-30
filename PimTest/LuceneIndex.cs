// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-20
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ru;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace PimTest
{
    public class LuceneIndex : ILuceneIndex, IDisposable
    {
        private const string WriteLockFileName = "write.lock";

        public Lucene.Net.Store.Directory Directory { get; private set; }

        public Analyzer Analyzer { get; private set; }

        public LuceneIndex(Analyzer analyzer, Lucene.Net.Store.Directory fullTextDirectory, string documentKeyName = "Id")
        {
            KeyFieldName = documentKeyName;
            Analyzer = analyzer;
            Directory = fullTextDirectory;

            using (var writer = CreateWriter())
            {
                CommitAndRefreshStats(writer, false);
            }
        }

        /// <summary>
        ///     Name to pass to <see cref="Document.Get(string)"/> to get document's 'primary key'
        /// </summary>
        public string KeyFieldName { get; private set; }

        public int DocCount { get; private set; }
        public int DeletedDocCount { get; private set; }

        public void Add(params Document[] docs)
        {
            Add(docs.AsEnumerable());
        }

        public void Add(IEnumerable<Document> items)
        {
            using (var writer = CreateWriter())
            {
                foreach (var item in items)
                {
                    writer.UpdateDocument(new Term(KeyFieldName, item.Get(KeyFieldName)), item);
                }

                CommitAndRefreshStats(writer);
            }
        }

        public void Delete(string key)
        {
            Delete(new Term(KeyFieldName, key));
        }

        public void Delete(params Term[] terms)
        {
            using (var writer = CreateWriter())
            {
                writer.DeleteDocuments(terms);

                CommitAndRefreshStats(writer);
            }
        }

        public void Delete(params Query[] queries)
        {
            using (var writer = CreateWriter())
            {
                writer.DeleteDocuments(queries);

                CommitAndRefreshStats(writer);
            }
        }

        public void DeleteAll()
        {
            using (var writer = CreateWriter())
            {
                writer.DeleteAll();

                CommitAndRefreshStats(writer);
            }
        }

        public IndexSearcher CreateSearcher(bool readOnly, bool calcScore)
        {
            var result = new IndexSearcher(Directory, readOnly);
            if (calcScore)
                result.SetDefaultFieldSortScoring(true, true);
            return result;
        }

        public List<SearchHit> Search(Query query, int maxResults)
        {
            using (var search = CreateSearcher(true, true))
            {
                var hits = search.Search(query, null, maxResults, Sort.RELEVANCE).ScoreDocs;

                return hits.Select(h => new SearchHit(search.Doc(h.Doc), h.Score)).ToList();
            }
        }

        public void CleanupDeletes()
        {
            using (var writer = CreateWriter())
            {
                writer.ExpungeDeletes();
                writer.Commit();
            }
        }

        public void Optimize()
        {
            using (var writer = CreateWriter())
            {
                writer.Optimize();
            }
        }

        private IndexWriter CreateWriter()
        {
            return new IndexWriter(Directory, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        /// <summary>
        ///     To be called after each update so that stats are reported correctly.
        /// </summary>
        private void CommitAndRefreshStats(IndexWriter writer, bool commit = true)
        {
            if (commit)
                writer.Commit();
            DocCount = writer.NumDocs();
        }

        public static FSDirectory PreparePersistentDirectory(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var dir = FSDirectory.Open(directoryInfo);

            if (IndexWriter.IsLocked(dir))
                IndexWriter.Unlock(dir);

            var lockFilePath = Path.Combine(directoryPath, WriteLockFileName);

            if (File.Exists(lockFilePath))
                File.Delete(lockFilePath);

            return dir;
        }

        public static RAMDirectory CreateTransientDirectory()
        {
            return new RAMDirectory();
        }

        public static Analyzer CreateDefaultAnalyzer(string name)
        {
            return new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Version.LUCENE_30, name);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Directory != null)
                    {
                        Directory.Dispose();
                    }

                    if (Analyzer != null)
                    {
                        Analyzer.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LuceneIndex() {
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
        #endregion IDisposable Support

    }
}