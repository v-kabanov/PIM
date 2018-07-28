// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-26
// Comment  
// **********************************************************************************************/
// 

using System.Web.Mvc;
using AspNetPim.Models;
using FulltextStorageLib;
using Microsoft.Owin;

namespace AspNetPim.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin,Reader,Writer")]
    public class HomeController : Controller
    {
        public IOwinContext OwinContext { get; }
        public HomeController(INoteStorage noteStorage, IOwinContext owinContext)
        {
            NoteStorage = noteStorage;

            OwinContext = owinContext;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public INoteStorage NoteStorage { get; }

        private HomeViewModel CreateViewModel()
        {
            return new HomeViewModel(NoteStorage);
        }

        [Authorize(Roles = "Admin,Reader,Writer")]
        public ActionResult Index()
        {
            var model = CreateViewModel();

            model.LoadLatest();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Writer")]
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

        [HttpPost]
        [Authorize(Roles = "Admin,Writer")]
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
            return View();
        }
    }
}