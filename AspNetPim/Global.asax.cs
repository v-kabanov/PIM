using AuNoteLib;
using AuNoteLib.Util;
using Autofac;
using Autofac.Integration.Mvc;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AspNetPim
{
    public class FulltextIndexWorker
    {
        private const string SnowballStemmerNameEnglish = "English";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private object _syncRoot = new object();

        public IList<string> ConfiguredLanguages { get; }

        private List<string> _activeLanguages;

        private double _backgroundTaskProgress;
        private volatile string _backgroundTaskStatus;

        /// <summary>
        ///     Activated languages in configured order; null until started
        /// </summary>
        public IList<string> ActiveLanguages { get; private set; }

        public SearchEngine<INote, INoteHeader> SearchEngine { get; }

        public INoteStorage NoteStorage { get; }

        private void RebuildIndexes(string[] names)
        {
            lock (_syncRoot)
            {
                _backgroundTaskProgress = 0;

                var allNotes = NoteStorage.GetAll();
                var numberOfItems = NoteStorage.CountAll();
                for (var indexIndex = 0; indexIndex < names.Length; ++indexIndex)
                {
                    var name = names[indexIndex];
                    _backgroundTaskStatus = $"Rebuilding fulltext index {name}";

                    SearchEngine.RebuildIndex(name, allNotes, (n) => _backgroundTaskProgress = ((double)indexIndex * numberOfItems + n) / (names.Length * numberOfItems));
                }
            }
        }

        public FulltextIndexWorker(string commaSeparatedLanguageNames, SearchEngine<INote, INoteHeader> searchEngine, INoteStorage storage)
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

        }

        private void EnsureFulltextIndex()
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
    }

    public class MvcApplication : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string AppSettingKeyFulltextIndexLanguages = "FulltextIndexLanguages";

        private ISet<string> _newIndexNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public SearchEngine<INote, INoteHeader> SearchEngine { get; private set; }

        private void ConfigureBackend()
        {
            var appDataPath = Server.MapPath("~/App_Data");
            var storageRootPath = Path.Combine(appDataPath, "UserData");

            var fullTextFolder = Path.Combine(storageRootPath, "ft");
            var dbFolder = Path.Combine(storageRootPath, "db");

            var storage = new NoteStorage(dbFolder);
            var adapter = new LuceneNoteAdapter();
            SearchEngine = new SearchEngine<INote, INoteHeader>(fullTextFolder, adapter, new MultiIndex(adapter.DocumentKeyName));

            var builder = new ContainerBuilder();

            builder.Register<INoteStorage>(context => storage).SingleInstance();
            builder.Register(context => SearchEngine).SingleInstance();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
