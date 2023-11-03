// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-26
// Comment  
// **********************************************************************************************/
// 

using System.Threading.Tasks;
using PimWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PimWeb.AppCode;

namespace PimWeb.Controllers;

public class HomeController : Controller
{
    public HomeController(INoteService noteService)
    {
        NoteService = noteService;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public INoteService NoteService { get; }

    [Authorize(Roles = "Admin,Writer")]
    public async Task<ActionResult> Index()
    {
        var model = await NoteService.GetLatestAsync();

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<ActionResult> DeleteNote(int noteId)
    {
        if (noteId > 0)
        {
            var note = await NoteService.DeleteAsync(new NoteViewModel { NoteId = noteId }, true);

            ViewBag.Message = note == null
                ? $"Note {noteId} was not found"
                : $"Note {noteId} successfully deleted";
        }

        var model = await NoteService.GetLatestAsync();

        return PartialView("IndexPartial", model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public async Task<ActionResult> Create(HomeViewModel model)
    {
        var result = await NoteService.CreateAsync(model);

        return PartialView("IndexPartial", result);
    }

    public ActionResult About()
    {
        return View();
    }
}