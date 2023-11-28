// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-12
// Comment  
// **********************************************************************************************/
// 

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PimWeb.Models;

public class NoteViewModel : SearchableEntityViewModelBase
{
    [DisplayName("Text")]
    [Required(AllowEmptyStrings = false)]
    public string NoteText { get; set; }
    
    public string Caption { get; set; }
    
    public List<FileViewModel> Files { get; set; } = new ();
}