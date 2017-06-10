// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-09
// Comment  
// **********************************************************************************************/

using System.Collections.Generic;
using FulltextStorageLib;

namespace AspNetPim.Models
{
    public class HomeSearchViewModel
    {
        public string SearchQuery { get; set; }

        public IList<NoteHeader> SearchResult { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }

    public class HomeViewModel
    {
        public HomeSearchViewModel HomeSearchViewModel { get; set; }


    }
}