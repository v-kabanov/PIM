// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PimWeb.AppCode;
using PimWeb.Models;

namespace PimWeb.Controllers;

[Authorize(Roles = "Admin,Reader,Writer")]
public class SearchController : Controller
{
    public INoteService NoteService { get; }

    public SearchController(INoteService noteService)
    {
        NoteService = noteService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Reader,Writer")]
    [Route("~/Search")]
    public ActionResult Search(SearchViewModel model)
    {
        model.Initialize(NoteService);

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
        model.Initialize(NoteService);

        model.Delete();

        if (ModelState.IsValid)
            model.ExecuteSearch();

        return PartialView("SearchPartial", model);
    }
}