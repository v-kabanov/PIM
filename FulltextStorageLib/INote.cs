// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-04
// Comment		
// **********************************************************************************************/

namespace FulltextStorageLib;

/// <summary>
///     Interface exposed to GUI
/// </summary>
public interface INote : INoteHeader
{
    string Text { get; set; }

    bool IsTransient { get; }

    int Version { get; }
}

public interface IPersistentNote : INote
{
    //new int Version { get; set; }

    int IncrementVersion();
}