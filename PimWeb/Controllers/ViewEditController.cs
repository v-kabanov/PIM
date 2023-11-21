// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-12
// Comment  
// **********************************************************************************************/
// 

using System.Threading.Tasks;
using PimWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PimWeb.AppCode;

namespace PimWeb.Controllers;

[Authorize(Roles = "Admin,Writer,Reader")]
public class ViewEditController : Controller
{
    private const string PartialViewName = "ViewEditPartial";

    // ReSharper disable once MemberCanBePrivate.Global
    public INoteService NoteService { get; }

    public ViewEditController(INoteService noteService)
    {
        NoteService = noteService;
    }

    [HttpGet]
    [Route("~/ViewEdit/{id}")]
    public async Task<ActionResult> Index(int id, bool edit = false)
    {
        var model = await NoteService.GetAsync(id).ConfigureAwait(false);
        ViewBag.Edit = edit;

        return View("ViewEdit", model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<PartialViewResult> Update(NoteViewModel model)
    {
        ModelState[nameof(model.Version)].RawValue = null;
        ViewBag.Edit = true;
        
        var result = model;
        try
        {
            result = await NoteService.SaveOrUpdateAsync(model);
        }
        catch (OptimisticConcurrencyException ex)
        {
            ModelState.AddModelError("", "Concurrent update detected, save your changes elsewhere, refresh the form and merge.");
        }

        return PartialView(PartialViewName, result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<ActionResult> Delete(NoteViewModel model)
    {
        var node = await NoteService.DeleteAsync(model, false);
        
        if (node == null)
        {
            ModelState.AddModelError("", $"Note #{model.Id} does not exist.");

            return View("Error");
        }

        return RedirectToAction("Index", "Home");
    }
}