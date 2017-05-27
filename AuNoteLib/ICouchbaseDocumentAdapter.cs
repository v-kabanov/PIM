// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-27
// Comment  
// **********************************************************************************************/
// 
using System.Collections.Generic;
using Couchbase.Lite;

namespace AuNoteLib
{
    public interface ICouchbaseDocumentAdapter<TDoc>
    {
        string GetId(TDoc document);

        IDictionary<string, object> ToDictionary(TDoc document);

        TDoc Read(Document document);

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
        ///     Optional, increments integrity version if supported.
        /// </summary>
        /// <param name="document">
        ///     Mandatory
        /// </param>
        /// <returns>
        ///     New version; 0 if not supported.
        /// </returns>
        int IncrementVersion(TDoc document);
    }
}