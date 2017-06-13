// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

using System.Web.Mvc;
using AspNetPim.Models;
using FulltextStorageLib;

namespace AspNetPim.Controllers
{
    [Route("~/Search/{action=Search}")]
    public class SearchController : Controller
    {
        public INoteStorage NoteStorage { get; }

        public SearchController(INoteStorage noteStorage)
        {
            NoteStorage = noteStorage;
        }

        [HttpGet]
        public ActionResult Search(SearchViewModel model)
        {
            model.Initialize(NoteStorage);

            if (ModelState.IsValid)
                model.ExecuteSearch();

            return View("Search", model);
        }

        [HttpPost]
        public ActionResult Delete(SearchViewModel model, string noteId)
        {
            model.Initialize(NoteStorage);

            if (!string.IsNullOrWhiteSpace(noteId))
                model.Delete(noteId);

            if (ModelState.IsValid)
                model.ExecuteSearch();

            return View("Search", model);
        }
    }
}