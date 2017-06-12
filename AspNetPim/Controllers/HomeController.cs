// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-26
// Comment  
// **********************************************************************************************/
// 

using System.Web.Mvc;
using AspNetPim.Models;
using FulltextStorageLib;

namespace AspNetPim.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(INoteStorage noteStorage)
        {
            NoteStorage = noteStorage;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public INoteStorage NoteStorage { get; }

        private HomeViewModel CreateViewModel()
        {
            return new HomeViewModel(NoteStorage);
        }

        public ActionResult Index()
        {
            var model = CreateViewModel();

            model.LoadLatest();

            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteNote(string noteId)
        {
            var model = CreateViewModel();

            if (!string.IsNullOrWhiteSpace(noteId))
            {
                var note = model.Delete(noteId);

                ViewBag.Message = note == null
                    ? $"Note {noteId} was not found"
                    : $"Note {noteId} successfully deleted";
            }

            model.LoadLatest();

            return PartialView("IndexPartial", model);
        }

        public ActionResult OpenNote(string noteId)
        {
            var model = CreateViewModel();

            var note = model.NoteStorage.GetExisting(noteId);

            ViewBag.Message = note == null
                ? $"Note {noteId} was not found"
                : note.Text;

            model.LoadLatest();

            return View("Index", model);
        }

        [HttpPost]
        public ActionResult Create(HomeViewModel model)
        {
            model.Initialize(NoteStorage);

            if (!string.IsNullOrWhiteSpace(model.NewNoteText))
                model.CreateNew();

            model.LoadLatest();

            return PartialView("IndexPartial", model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult ViewEdit(string id)
        {
            var model = new NoteViewModel(NoteStorage) {NoteId = id};

            model.Load();

            return View(model);
        }

        [HttpPost]
        public ActionResult Update(NoteViewModel model)
        {
            model.Initialize(NoteStorage);

            model.Update();

            return PartialView("ViewEditPartial", model);
        }
    }
}