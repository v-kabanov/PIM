// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-12
// Comment  
// **********************************************************************************************/
// 
using System.Web.Mvc;
using AspNetPim.Models;
using FulltextStorageLib;

namespace AspNetPim.Controllers
{
    [Route("~/ViewEdit/{id}/{action=Index}")]
    [Authorize(Roles = "Admin,Writer,Reader")]
    public class ViewEditController : Controller
    {
        private const string PartialViewName = "ViewEditPartial";

        // ReSharper disable once MemberCanBePrivate.Global
        public INoteStorage NoteStorage { get; }

        public ViewEditController(INoteStorage noteStorage)
        {
            NoteStorage = noteStorage;
        }

        [HttpGet]
        public ActionResult Index(string id)
        {
            var model = new NoteViewModel(NoteStorage) { NoteId = id };

            model.Load();

            return View("ViewEdit", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Writer")]
        public ActionResult Update(NoteViewModel model)
        {
            model.Initialize(NoteStorage);

            model.Update();

            return PartialView(PartialViewName, model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Writer")]
        public ActionResult Delete(NoteViewModel model)
        {
            model.Initialize(NoteStorage);

            model.Delete();

            return RedirectToAction("Index", "Home");
        }
    }
}