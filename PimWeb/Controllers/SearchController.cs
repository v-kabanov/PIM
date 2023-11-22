// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-13
// Comment  
// **********************************************************************************************/

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pim.CommonLib;
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
    public async Task<ActionResult> Search(NoteSearchViewModel model)
    {
        var result = model;

        Validate(model);

        result = await NoteService.SearchAsync(model, false);

        return View("Search", result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<PartialViewResult> Delete(NoteSearchViewModel model)
    {
        var result = model;
        
        if (ModelState.IsValid)
            result = await NoteService.SearchAsync(model, true);

        return PartialView("SearchPartial", result);
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin,Reader,Writer")]
    [Route("~/files/{id}")]
    public async Task<ActionResult> GetFile(int id)
    {
        var model = await NoteService.GetFileAsync(id);
        if (!model.ExistsOnDisk)
        {
            ModelState.AddModelError("", "File does not exist");
            return View("Error");
        }
        
        return File(new FileStream(model.FullPath, FileMode.Open), model.MimeType);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Reader,Writer")]
    [Route("~/files")]
    public async Task<ActionResult> SearchFiles(FileSearchViewModel model)
    {
        var result = model;

        Validate(model);
        
        result = await NoteService.SearchAsync(model, false);

        return View("Search", result);
    }
    
    private void Validate(SearchModelBase model)
    {
        if (model.Query.IsNullOrWhiteSpace() && model.SortProperty == SortProperty.SearchRank)
            ModelState.AddModelError(nameof(model.SortProperty), "Search Rank is not available without query, sorting by Last Update Time.");

        if (model.LastUpdatePeriodStart >= model.LastUpdatePeriodEnd)
            ModelState.AddModelError(nameof(model.LastUpdatePeriodEnd), "Period end date must be greater than start.");
    }
}