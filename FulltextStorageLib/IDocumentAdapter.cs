// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-15
// Comment  
// **********************************************************************************************/
// 

using System.Collections.Generic;

namespace FulltextStorageLib;

public interface IDocumentAdapter<TDoc>
{
    string GetId(TDoc document);

    bool IsTransient(TDoc document);

    /// <summary>
    ///     Check if 2 versions of the same document are logically different from the point of view of needing to update in the database.
    /// </summary>
    /// <param name="version1">
    ///     Mandatory
    /// </param>
    /// <param name="version2">
    ///     Mandatory
    /// </param>
    bool IsChanged(TDoc version1, TDoc version2);

    /// <summary>
    ///     Optional, marks document as having been updated now,
    ///     increments integrity version if supported and last update time.
    /// </summary>
    /// <param name="document">
    ///     Mandatory
    /// </param>
    /// <returns>
    ///     New version; 0 if not supported.
    /// </returns>
    int IncrementVersion(TDoc document);
    IDictionary<string, object> ToDictionary(TDoc document);
}