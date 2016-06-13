using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Navigation;
using AuNoteLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using Lucene.Net.Store;
using NFluent;

namespace AuNote.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string AppSettingKeyFulltextIndexLanguages = "FulltextIndexLanguages";
        private const string SnowballStemmerNameEnglish = "English";

        private NoteStorage Storage { get; set; }
        private SearchEngine<INote, INoteHeader> SearchEngine { get; set; }

        private int NoteListSize => 20;

        public static string DataRootPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AuNotes");

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                NoteHeaders = new ObservableCollection<INoteHeader>()
                {
                    new NoteHeader() { CreateTime = DateTime.Now.AddDays(-10), Id = Note.CreateShortGuid(), Name = "First note" },
                    new NoteHeader() { CreateTime = DateTime.Now.AddDays(-10), Id = Note.CreateShortGuid(), Name = "Second note" },
                };
            }
            else
            {
                // Code runs "for real"
                var fullTextFolder = Path.Combine(DataRootPath, "ft");
                var dbFolder = Path.Combine(DataRootPath, "db");

                Storage = new NoteStorage(dbFolder);
                var adapter = new LuceneNoteAdapter();
                SearchEngine = new SearchEngine<INote, INoteHeader>(fullTextFolder, adapter, new MultiIndex(adapter.DocumentKeyName));


                var configuredLanguageNames = ConfigurationManager.AppSettings[AppSettingKeyFulltextIndexLanguages]?.Split(',').Select(s => s.Trim()).ToList();

                HashSet<string> allSupportedLangs = new HashSet<string>(LuceneIndex.GetAvailableSnowballStemmers());

                var unknownLangs = string.Join(",", configuredLanguageNames.Where(n => !allSupportedLangs.Contains(n)));
                if (!string.IsNullOrEmpty(unknownLangs))
                {
                    Log.Error($"The following configured languages are not supported: {unknownLangs}");
                }

                EnsureFulltextIndex(SnowballStemmerNameEnglish);

                NoteHeaders = new ObservableCollection<INoteHeader>(SearchEngine.GetTopInPeriod(null, null, NoteListSize));
            }

            AddNoteCommand = new RelayCommand(CreateNewNote, CanSaveNewNote);
            RefreshSearchCommand = new RelayCommand(RefreshSearch);
        }

        public string SearchText { get; set; }

        public ICommand AddNoteCommand { get; set; }

        public ICommand RefreshSearchCommand { get; set; }

        public ObservableCollection<INoteHeader> NoteHeaders { get; set; }

        public void Delete(params INoteHeader[] headers)
        {
            foreach (var noteHeader in headers)
            {
                Log.Debug($"Deleting #{noteHeader.Id}: {noteHeader.Name}");
                Storage.Delete(noteHeader.Id);
            }
        }

        public void CreateNewNote()
        {
            
        }

        public void RefreshSearch()
        {
            
        }

        public bool CanSaveNewNote()
        {
            return !IsInDesignMode;
        }

        /// <summary>
        ///     Create and populate new index if it does not yet exist;
        ///     do nothing otherwise.
        /// </summary>
        /// <param name="snowballStemmerName">
        ///     Name of the snowball stemmer (language) which is also used as index name. See <see cref="LuceneIndex.GetAvailableSnowballStemmers"/>
        /// </param>
        private void EnsureFulltextIndex(string snowballStemmerName)
        {
            Check.That(snowballStemmerName).IsOneOfThese(LuceneIndex.GetAvailableSnowballStemmers().ToArray());

            if (SearchEngine.MultiIndex.GetIndex(snowballStemmerName) == null)
            {
                Log.Info($"Adding index {snowballStemmerName}");

                var analyzer = LuceneIndex.CreateSnowballAnalyzer(snowballStemmerName);

                SearchEngine.AddIndex(snowballStemmerName, analyzer);

                SearchEngine.RebuildIndex(snowballStemmerName, Storage.GetAll());
            }
            else
            {
                Log.Info($"EnsureFulltextIndex: {snowballStemmerName} index already exists");
            }
        }
    }
}