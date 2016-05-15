// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-04
// Comment		
// **********************************************************************************************/

using System;

namespace AuNoteLib
{
    public class NoteHeader : INoteHeader
    {
        public string Id { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Name { get; set; }
    }
}