using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using AuNoteLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using Lucene.Net.Store;

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

        private NoteStorage Storage { get; set; }
        private SearchEngine<INote, INoteHeader> SearchEngine { get; set; }

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

                NoteHeaders = new ObservableCollection<INoteHeader>(SearchEngine.GetTopInPeriod(null, null, 20));
            }

            AddNoteCommand = new RelayCommand(CreateNewNote, CanSaveNewNote);
            RefreshSearchCommand = new RelayCommand(RefreshSearch);
        }

        public string SearchText { get; set; }

        public ICommand AddNoteCommand { get; set; }

        public ICommand RefreshSearchCommand { get; set; }

        public ObservableCollection<INoteHeader> NoteHeaders { get; set; }

        

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
    }
}