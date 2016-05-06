// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-04
// Comment		
// **********************************************************************************************/

using System;

namespace PimTest
{
    /// <summary>
    ///     Interface exposed to GUI
    /// </summary>
    public interface INote : INoteHeader
    {
        DateTime LastUpdateTime { get; }

        string Text { get; set; }

        bool IsTransient { get; }

        int Version { get; }
    }

    public interface IPersistentNote : INote
    {
        //new int Version { get; set; }

        int IncrementVersion();
    }
}