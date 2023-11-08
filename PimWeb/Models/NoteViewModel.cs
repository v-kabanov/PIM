// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-12
// Comment  
// **********************************************************************************************/
// 

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PimWeb.Models;

public class NoteViewModel
{
    public int NoteId { get; set; }

    [DisplayName("Text")]
    [Required(AllowEmptyStrings = false)]
    public string NoteText { get; set; }
    
    public string Caption { get; set; }

    [DisplayFormat(DataFormatString = "{0:f}")]
    [DisplayName("Creation Time")]
    public DateTimeOffset? CreateTime { get; set; }

    [DisplayName("Last Update Time")]
    [DisplayFormat(DataFormatString = "{0:f}")]
    public DateTimeOffset? LastUpdateTime { get; set; }

    [DisplayName("Version")]
    public int? Version { get; set; }
    
    [DisplayName("Headline")]
    public string SearchHeadline { get; set; }

    public float? Rank { get; set; }
    
    public bool NoteDeleted { get; set; }
}