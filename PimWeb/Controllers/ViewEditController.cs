// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-12
// Comment  
// **********************************************************************************************/
// 

using PimWeb.Models;
using FulltextStorageLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PimWeb.Controllers;

[Authorize(Roles = "Admin,Writer,Reader")]
public class ViewEditController : Controller
{
    private const string PartialViewName = "ViewEditPartial";

    // ReSharper disable once MemberCanBePrivate.Global
    public INoteStorage NoteStorage { get; }

    public ViewEditController(INoteStorage noteStorage)
    {
        NoteStorage = noteStorage;
    }

    [HttpGet]
    [Route("~/ViewEdit/{id}")]
    public ActionResult Index(string id)
    {
        var model = new NoteViewModel(NoteStorage) { NoteId = id };

        model.Load();

        return View("ViewEdit", model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public ActionResult Update(NoteViewModel model)
    {
        model.Initialize(NoteStorage);

        model.Update();

        return PartialView(PartialViewName, model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Writer")]
    public ActionResult Delete(NoteViewModel model)
    {
        model.Initialize(NoteStorage);

        model.Delete();

        return RedirectToAction("Index", "Home");
    }
}