// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-12
// Comment  
// **********************************************************************************************/
// 

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
        var model = await NoteService.GetNoteAsync(id).ConfigureAwait(false);
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
        var note = await NoteService.DeleteAsync(model, false);
        
        if (note == null)
        {
            ModelState.AddModelError("", $"Note #{model.Id} does not exist.");

            return View("Error");
        }

        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet("~/file/{id}")]
    [Authorize(Roles = "Admin,Reader,Writer")]
    public async Task<ActionResult> File(int id, bool edit = false)
    {
        var model = await NoteService.GetFileAsync(id).ConfigureAwait(false);
        ViewBag.Edit = edit;

        return View(model);
    }
    
    [HttpPost("~/ViewEdit/{noteId}/upload-file")]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<PartialViewResult> UploadFileForNote(int noteId, IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var result = await NoteService.UploadFileForNote(noteId, file.FileName, ms.ToArray());

        return PartialView("FileList", result);
    }

    [HttpPost("~/ViewEdit/{noteId}/unlink-file")]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<PartialViewResult> UnlinkFile(int noteId, int fileId)
    {
        var result = await NoteService.UnlinkFileFromNote(noteId, fileId);

        return PartialView("FileList", result);
    }
}