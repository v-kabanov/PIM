// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-26
// Comment  
// **********************************************************************************************/
// 

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

    private HomeViewModel CreateViewModel()
    {
        return new HomeViewModel(NoteService);
    }

    [Authorize(Roles = "Admin,Writer")]
    public ActionResult Index()
    {
        var model = CreateViewModel();

        model.LoadLatest();

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public ActionResult DeleteNote(int noteId)
    {
        var model = CreateViewModel();

        if (noteId > 0)
        {
            var note = model.Delete(noteId);

            ViewBag.Message = note == null
                ? $"Note {noteId} was not found"
                : $"Note {noteId} successfully deleted";
        }

        model.LoadLatest();

        return PartialView("IndexPartial", model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public ActionResult Create(HomeViewModel model)
    {
        model.Initialize(NoteService);

        if (!string.IsNullOrWhiteSpace(model.NewNoteText))
            model.CreateNew();

        model.LoadLatest();

        return PartialView("IndexPartial", model);
    }

    public ActionResult About()
    {
        return View();
    }
}