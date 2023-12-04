using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PimWeb.Models;

public class SearchableEntityViewModelBase
{
    public int Id { get; set; }

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
    
    public bool IfDeleted { get; set; }
    
    public bool IfSelectDisabled { get; set; }
}