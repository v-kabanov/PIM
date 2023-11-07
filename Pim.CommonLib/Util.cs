// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-23
// Comment  
// **********************************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Pim.CommonLib;

public static class Util
{
    private static readonly Lazy<Regex> LazySplitPascalCaseRegex = new (() => new Regex(@"([\s]|(?<=[a-z])(?=[A-Z]|[0-9])|(?<=[A-Z])(?=[A-Z][a-z]|[0-9])|(?<=[0-9])(?=[^0-9]))"));

    /// <summary>
    ///     Captures empty strings between text components
    /// </summary>
    public static Regex SplitPascalCaseRegex => LazySplitPascalCaseRegex.Value;

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

    /// <summary>
    ///     Split pascal-case string into components
    /// </summary>
    /// <param name="input">
    ///     Mandatory
    /// </param>
    /// <returns>
    ///     array containing non-blank-space strings
    /// </returns>
    public static string[] SplitPascalCase(string input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        return SplitPascalCaseRegex.Split(input).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
    }

    /// <summary>
    ///     For "GreatBarrierReef12" returns "Great Barrier Reef 12"
    /// </summary>
    /// <param name="input">
    ///     Mandatory
    /// </param>
    /// <param name="delimiter">
    ///     Mandatory
    /// </param>
    public static string InsertDelimitersIntoPascalCaseString(string input, string delimiter = " ")
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        if (delimiter == null) throw new ArgumentNullException(nameof(delimiter));

        return SplitPascalCaseRegex.Replace(input, delimiter);
    }
}