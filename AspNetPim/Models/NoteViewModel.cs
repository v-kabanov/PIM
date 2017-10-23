// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-12
// Comment  
// **********************************************************************************************/
// 
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FulltextStorageLib;
using FulltextStorageLib.Util;
using Pim.CommonLib;

namespace AspNetPim.Models
{
    public class NoteViewModel
    {
        public string NoteId { get; set; }

        [DisplayName("Text")]
        [Required(AllowEmptyStrings = false)]
        [AllowHtml]
        public string NoteText { get; set; }

        [DisplayFormat(DataFormatString = "{0:f}")]
        [DisplayName("Creation Time")]
        public DateTime? CreateTime => Note?.CreateTime;

        [DisplayName("Last Update Time")]
        [DisplayFormat(DataFormatString = "{0:f}")]
        public DateTime? LastUpdateTime => Note?.LastUpdateTime;

        [DisplayName("Version")]
        public int? Version => Note?.Version;

        public bool NoteDeleted { get; private set; }

        public INoteStorage NoteStorage { get; private set; }

        public Note Note { get; private set; }

        public NoteViewModel()
        {
        }

        public NoteViewModel(INoteStorage noteStorage)
        {
            NoteStorage = noteStorage;
        }

        public void Initialize(INoteStorage noteStorage)
        {
            NoteStorage = noteStorage ?? DependencyResolver.Current.GetService<INoteStorage>();
        }

        public void Load()
        {
            Note = ReadFromStorage();

            NoteText = Note.Text;
        }

        public void Update()
        {
            var newText = NoteText?.Trim();

            Check.DoCheckOperationValid(!string.IsNullOrEmpty(newText), () => "Note text must not be empty.");

            if (Note == null)
                Note = ReadFromStorage();

            if (Note.Text != newText)
            {
                Note.Text = newText;
                Note.LastUpdateTime = DateTime.Now;

                NoteStorage.SaveOrUpdate(Note);
            }
        }

        public void Delete()
        {
            Note = NoteStorage.Delete(NoteId);

            Check.DoEnsureLambda(Note != null, () => $"Note {NoteId} does not exist");

            NoteDeleted = true;
        }

        private Note ReadFromStorage()
        {
            var result = NoteStorage.GetExisting(NoteId);

            Check.DoEnsureLambda(result != null, () => $"Note {NoteId} does not exist");

            return result;
        }
    }
}