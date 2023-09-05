using System;

namespace FulltextStorageLib.QueryBuilding;

class TimeFilter : QueryTerm
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}