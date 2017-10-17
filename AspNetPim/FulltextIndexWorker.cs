// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-05-24
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using FulltextStorageLib;
using FulltextStorageLib.Util;
using log4net;

namespace AspNetPim
{
    public class FulltextIndexWorker : IDisposable
    {
        private const string SnowballStemmerNameEnglish = "English";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object _syncRoot = new object();

        private readonly TaskExecutor _backgroundTaskExecutor = new TaskExecutor();

        public IList<string> ConfiguredLanguages { get; }

        private List<string> _activeLanguages;

        private double _backgroundTaskProgress;
        private volatile string _backgroundTaskStatus;

        /// <summary>
        ///     Activated languages in configured order; null until started
        /// </summary>
        public IList<string> ActiveLanguages { get; private set; }

        public SearchEngine<INote, INoteHeader, string> SearchEngine { get; }

        public INoteStorage NoteStorage { get; }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private void RebuildIndexes(string[] names)
        {
            lock (_syncRoot)
            {
                _backgroundTaskProgress = 0;

                var allNotes = NoteStorage.GetAll();
                var numberOfItems = NoteStorage.CountAll();

                SearchEngine.RebuildIndexes(names, allNotes, numberOfItems, d => _backgroundTaskProgress = d);
            }
        }

        public FulltextIndexWorker(string commaSeparatedLanguageNames, SearchEngine<INote, INoteHeader, string> searchEngine, INoteStorage storage)
        {
            Check.DoRequireArgumentNotNull(searchEngine, nameof(searchEngine));
            Check.DoRequireArgumentNotNull(storage, nameof(storage));

            commaSeparatedLanguageNames = commaSeparatedLanguageNames ?? string.Empty;

            NoteStorage = storage;
            SearchEngine = searchEngine;

            ConfiguredLanguages = commaSeparatedLanguageNames.Split(',').Select(n => n.Trim()).Where(n => !string.IsNullOrEmpty(n))
                .ToList().AsReadOnly();
        }

        public void Start()
        {
            lock (_syncRoot)
            {

            }
        }

        private void ActivateIndexes()
        {
            var languageNames = ConfiguredLanguages.ToCaseInsensitiveSet();

            var allSupportedLangs = LuceneIndex.GetAvailableSnowballStemmers().ToCaseInsensitiveSet();

            var unknownLangs = string.Join(",", languageNames.Where(n => !allSupportedLangs.Contains(n)));

            if (!string.IsNullOrEmpty(unknownLangs))
                Log.ErrorFormat("The following configured languages are not supported: {0}", unknownLangs);

            languageNames.IntersectWith(allSupportedLangs);

            if (languageNames.Count == 0)
                _activeLanguages = SnowballStemmerNameEnglish.WrapInList();
            else
                _activeLanguages = ConfiguredLanguages.Where(languageNames.Contains).ToList();

            ActiveLanguages = _activeLanguages.AsReadOnly();

            var newIndexes = new List<IndexInformation>();

            foreach (var stemmerName in ActiveLanguages)
            {
                var info = SearchEngine.AddOrOpenSnowballIndex(stemmerName);
                if (info.IsNew)
                    newIndexes.Add(info);
            }

            if (languageNames.Count > 1)
                SearchEngine.SetDefaultIndex(ActiveLanguages.First());

            if (newIndexes.Count > 0)
            {
                _backgroundTaskExecutor.Schedule(() => RebuildIndexes(newIndexes.Select(x => x.Name).ToArray()));
            }
        }

        /// <summary>
        ///     Create and populate new index if it does not yet exist;
        ///     do nothing otherwise.
        /// </summary>
        /// <param name="snowballStemmerName">
        ///     Name of the snowball stemmer (language) which is also used as index name. See <see cref="LuceneIndex.GetAvailableSnowballStemmers"/>
        /// </param>
        private IndexInformation EnsureFulltextIndex(string snowballStemmerName)
        {
            Contract.Requires(!string.IsNullOrEmpty(snowballStemmerName));
            Contract.Requires(LuceneIndex.GetAvailableSnowballStemmers().Contains(snowballStemmerName));

            Log.InfoFormat($"Adding index {0}", snowballStemmerName);

            return SearchEngine.AddOrOpenSnowballIndex(snowballStemmerName);

        }

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    if (_backgroundTaskExecutor != null)
                    {
                        _backgroundTaskExecutor.StopAcceptingNewTasks();
                        _backgroundTaskExecutor.WaitForAllCurrentTasksToFinish(20000);
                        _backgroundTaskExecutor.Dispose();
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FulltextIndexWorker() {
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
    }
}