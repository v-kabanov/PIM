// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-01-13
// Comment  
// **********************************************************************************************/
// 

using System.IO;
using Pim.CommonLib;

namespace FulltextStorageLib.Util;

/// <summary>
///     Simple file deletion task, no exception handling, use <see cref="TryTask"/> etc if needed.
/// </summary>
public class DeleteFileTask : ITask
{
    public string FilePath { get; }

    public DeleteFileTask(string filePath)
    {
        Check.DoRequireArgumentNotNull(filePath, nameof(filePath));

        FilePath = filePath;
        Name = $"Delete {Path.GetFileName(filePath)}";
    }

    public void Execute()
    {
        File.Delete(FilePath);
    }

    public string Name { get; }
}