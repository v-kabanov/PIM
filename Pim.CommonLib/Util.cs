// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-23
// Comment  
// **********************************************************************************************/

using System;
using System.IO;
using System.Reflection;

namespace Pim.CommonLib;

public static class Util
{
    /// <summary>
    ///     Expand path relative to the directory containing executing assembly if possible.
    /// </summary>
    /// <param name="relativePath">
    ///     Mandatory
    /// </param>
    /// <returns>
    ///     <paramref name="relativePath"/> if executing assembly cannot be 
    /// </returns>
    public static string GetFullPathNextToExecutable(string relativePath)
    {
        Check.DoRequireArgumentNotNull(relativePath, nameof(relativePath));

        var exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return exeDirectory == null
            ? relativePath
            : Path.Combine(exeDirectory, relativePath);
    }

    public static string CreateShortGuid()
    {
        var encoded = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        encoded = encoded
            .Replace("/", "_")
            .Replace("+", "-");

        return encoded.Substring(0, 22);
    }
}