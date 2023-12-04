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
    // ReSharper disable once MemberCanBePrivate.Global
    public INoteService NoteService { get; }

    public ViewEditController(INoteService noteService)
    {
        NoteService = noteService;
    }

    [HttpGet("~/note/{id:int}")]
    public async Task<ActionResult> Note(int id, bool edit = false)
    {
        var model = await NoteService.GetNoteAsync(id).ConfigureAwait(false);
        ViewBag.Edit = edit;

        return View("Note", model);
    }

    [HttpPost("~/note")]
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

        return PartialView("NotePartial", result);
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
    
    [HttpGet("~/note/{id:int}/attach-existing-files")]
    public async Task<ActionResult> AttachExistingFiles(int noteId)
    {
        var model = new AttachExistingFilesToNoteViewModel {Note = new NoteViewModel {Id = noteId} };
        
        model = await NoteService.ProcessAsync(model, false).ConfigureAwait(false);

        return View("AttachExistingFiles", model);
    }

    [HttpPost("~/note/attach-existing-files")]
    public async Task<PartialViewResult> AttachExistingFiles(AttachExistingFilesToNoteViewModel model, bool commit)
    {
        //model = await NoteService.ProcessAsync(model, false).ConfigureAwait(false);

        return PartialView("AttachExistingFilesPartial", model);
    }
    
    [HttpPost("~/file/{id}")]
    [Authorize(Roles = "Admin,Reader,Writer")]
    public async Task<ActionResult> File(int id, bool edit = false)
    {
        var model = await NoteService.GetFileAsync(id).ConfigureAwait(false);
        ViewBag.Edit = edit;

        return View(model);
    }

    [HttpPost("~/file/upload")]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<JsonResult> UploadFile(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var result = await NoteService.SaveFileAsync(file.FileName, ms.ToArray());

        return Json(new {Id = result.File.Id, IfDuplicate = result.Duplicate, Url = Url.Action("File", new {id = result.File.Id})});
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin,Reader,Writer")]
    [Route("~/files/{id}/download")]
    public async Task<ActionResult> DownloadFile(int id, bool viewInBrowser = true)
    {
        var model = await NoteService.GetFileAsync(id);
        if (!model.ExistsOnDisk)
        {
            ModelState.AddModelError("", "File does not exist");
            return View("Error");
        }
        
        var suggestedFileName = !viewInBrowser
            ? Path.ChangeExtension(model.Title, Path.GetExtension(model.RelativePath))
            : null;
        
        return File(new FileStream(model.FullPath, FileMode.Open), model.MimeType, suggestedFileName);
    }
    
    [HttpPost("~/ViewEdit/{noteId}/upload-file")]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<PartialViewResult> UploadFileForNote(int noteId, IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var result = await NoteService.UploadFileForNoteAsync(noteId, file.FileName, ms.ToArray());

        return PartialView("FileList", result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<JsonResult> DeleteFile(int id)
    {
        var result = await NoteService.DeleteAsync(new FileViewModel {Id = id});

        return Json(new {success = true});
    }

    [HttpPost("~/file/{id:int}")]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<PartialViewResult> UpdateFile(FileViewModel model)
    {
        ModelState[nameof(model.Version)].RawValue = null;
        ViewBag.Edit = true;
        
        var result = model;
        try
        {
            result = await NoteService.UpdateAsync(model);
        }
        catch (OptimisticConcurrencyException ex)
        {
            ModelState.AddModelError("", "Concurrent update detected, save your changes elsewhere, refresh the form and merge.");
        }

        return PartialView("FilePartial", result);
    }

    [HttpDelete("~/file/delete")]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<ActionResult> DeleteFilePermanently(FileViewModel model)
    {
        var file = await NoteService.DeleteAsync(model);
        
        if (file == null)
        {
            ModelState.AddModelError("", $"Note #{model.Id} does not exist.");

            return View("Error");
        }

        return RedirectToAction("Files", "Search");
    }

    [HttpPost("~/note/{noteId:int}/unlink-file")]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<PartialViewResult> UnlinkFileFromNote(int noteId, int fileId)
    {
        var result = await NoteService.UnlinkFileFromNote(noteId, fileId);

        return PartialView("FileList", result);
    }
    

    [HttpPost("~/file/{fileId:int}/unlink-note")]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<PartialViewResult> UnlinkNoteFromFile(int noteId, int fileId)
    {
        var result = await NoteService.UnlinkNoteFromFile(fileId, noteId);

        return PartialView("NoteList", result.Notes);
    }
}