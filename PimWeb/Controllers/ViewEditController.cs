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
    public ActionResult Index(int id)
    {
        var model = new NoteViewModel(NoteService) { NoteId = id };

        model.Load();

        return View("ViewEdit", model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public ActionResult Update(NoteViewModel model)
    {
        model.Initialize(NoteService);

        model.Update();

        return PartialView(PartialViewName, model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public ActionResult Delete(NoteViewModel model)
    {
        model.Initialize(NoteService);

        model.Delete();

        return RedirectToAction("Index", "Home");
    }
}