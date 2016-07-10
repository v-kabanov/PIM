using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
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

                var allSupportedLangs = new HashSet<string>(LuceneIndex.GetAvailableSnowballStemmers());

                var unknownLangs = string.Join(",", configuredLanguageNames.Where(n => !allSupportedLangs.Contains(n)));
                if (!string.IsNullOrEmpty(unknownLangs))
                {
                    Log.Error($"The following configured languages are not supported: {unknownLangs}");
                }

                var configuredSupportedLangs = configuredLanguageNames.Intersect(allSupportedLangs).ToList();

                if (configuredSupportedLangs.Count == 0)
                {
                    EnsureFulltextIndex(SnowballStemmerNameEnglish);
                }
                else
                {
                    // first in the list will become default
                    foreach (var stemmerName in configuredSupportedLangs)
                    {
                        EnsureFulltextIndex(stemmerName);
                    }
                }

                NoteHeaders = new ObservableCollection<INoteHeader>(SearchEngine.GetTopInPeriod(null, DateTime.Now, NoteListSize));
            }

            PositionDictionary = new Dictionary<string, int>();

            for (var n = 0; n < NoteHeaders.Count; ++n)
            {
                PositionDictionary.Add(NoteHeaders[n].Id, n);
            }

            AddNoteCommand = new RelayCommand(CreateNewNote, CanSaveNewNote);
            //RefreshSearchCommand = new RelayCommand(RefreshSearch);
        }

        private string _searchText;

        public string SearchText
        {
            get {return _searchText;}
            set
            {
                // TODO: refresh search
                Set(ref _searchText, value);
            }
        }

        public ICommand AddNoteCommand { get; set; }

        //public ICommand RefreshSearchCommand { get; set; }

        public ObservableCollection<INoteHeader> NoteHeaders { get; private set; }

        public IDictionary<string, int> PositionDictionary { get; private set; }

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
            MessageBox.Show("New nn", "New note");
        }

        /// <summary>
        ///     
        /// </summary>
        public void RefreshSearch()
        {
            var searchText = SearchText?.Trim();

            IList<INoteHeader> notes;

            if (!string.IsNullOrEmpty(searchText))
            {
                notes = SearchEngine.Search(searchText, NoteListSize);
            }
            else
            {
                notes = SearchEngine.GetTopInPeriod(null, null, NoteListSize);
            }

            MergeIntoExistingList(notes);
        }

        private void MergeIntoExistingList(IList<INoteHeader> notes)
        {
            Check.That(notes).IsNotNull();

            for (var n = 0; n < notes.Count; ++n)
            {
                var newNote = notes[n];

                var oldPosition = -1;
                if (PositionDictionary.TryGetValue(newNote.Id, out oldPosition))
                {
                    if (oldPosition != n)
                    {
                        NoteHeaders.Move(oldPosition, n);
                        PositionDictionary[newNote.Id] = n;
                    }
                    if (!AreEqualVersions(NoteHeaders[n], newNote))
                    {
                        NoteHeaders[n] = newNote;
                    }
                }
                else
                {
                    NoteHeaders.Insert(n, newNote);
                    PositionDictionary.Add(newNote.Id, n);
                }
            }

            while (NoteHeaders.Count > notes.Count)
            {
                var lastIndex = NoteHeaders.Count - 1;
                PositionDictionary.Remove(NoteHeaders[lastIndex].Id);
                NoteHeaders.RemoveAt(lastIndex);
            }
        }

        /// <summary>
        ///     Compares 2 versions of the same note header
        /// </summary>
        private bool AreEqualVersions(INoteHeader header1, INoteHeader header2)
        {
            Check.That(header1).IsNotNull();
            Check.That(header2).IsNotNull();
            Check.That(header1.Id).Equals(header2.Id);

            return header1.Name == header2.Name && header1.CreateTime == header2.CreateTime;
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