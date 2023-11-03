// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

using System.Threading.Tasks;
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
    public async Task<ActionResult> Search(SearchViewModel model)
    {
        var result = model;
        if (model.LastUpdatePeriodStart >= model.LastUpdatePeriodEnd)
            ModelState.AddModelError(nameof(model.LastUpdatePeriodEnd), "Period end date must be greater than start.");
        else if (ModelState.IsValid)
            result = await NoteService.SearchAsync(model, false);

        return View("Search", result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<ActionResult> Delete(SearchViewModel model)
    {
        var result = model;
        
        if (ModelState.IsValid)
            result = await NoteService.SearchAsync(model, true);

        return PartialView("SearchPartial", result);
    }
}