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
    [Authorize(Roles = "Admin,Reader,Writer")]
    public class SearchController : Controller
    {
        public INoteStorage NoteStorage { get; }

        public SearchController(INoteStorage noteStorage)
        {
            NoteStorage = noteStorage;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Reader,Writer")]
        public ActionResult Search(SearchViewModel model)
        {
            model.Initialize(NoteStorage);

            if (model.PeriodStart >= model.PeriodEnd)
            {
                ModelState.AddModelError(nameof(model.PeriodEnd), "Period end date must be greater than start.");
                //ModelState.AddModelError("", "Period end date must be greater than start.");
            }
            else if (ModelState.IsValid)
                model.ExecuteSearch();

            return View("Search", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Writer")]
        public ActionResult Delete(SearchViewModel model)
        {
            model.Initialize(NoteStorage);

            model.Delete();

            if (ModelState.IsValid)
                model.ExecuteSearch();

            return PartialView("SearchPartial", model);
        }
    }
}