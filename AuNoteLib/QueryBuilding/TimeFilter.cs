using System;

namespace AuNoteLib.QueryBuilding
{
    class TimeFilter : QueryTerm
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}