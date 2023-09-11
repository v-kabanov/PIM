// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

using FulltextStorageLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PimWeb.Models;

namespace PimWeb.Controllers;

//[Route("~/Search/{action}")]
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
    [Route("")]
    [Route("~/Search")]
    public ActionResult Search(SearchViewModel model)
    {
        model.Initialize(NoteStorage);

        if (model.PeriodStart >= model.PeriodEnd)
            ModelState.AddModelError(nameof(model.PeriodEnd), "Period end date must be greater than start.");
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