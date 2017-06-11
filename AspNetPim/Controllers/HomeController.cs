using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspNetPim.Models;

namespace AspNetPim.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = DependencyResolver.Current.GetService<HomeViewModel>();

            model.LoadLatest();

            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteNote(string noteId)
        {
            var model = DependencyResolver.Current.GetService<HomeViewModel>();

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
            var model = DependencyResolver.Current.GetService<HomeViewModel>();

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
    }
}