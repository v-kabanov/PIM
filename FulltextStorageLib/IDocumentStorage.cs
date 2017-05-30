// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-05-25
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;

namespace FulltextStorageLib
{
    public interface IDocumentStorage<TDoc, in TKey> : IDisposable
        where TDoc: class
    {
        /// <summary>
        ///     If note is transient, generates and assigns new Id and saves.<br/>
        ///     Saves if:<br/>
        ///         - note is transient<br/>
        ///         - note is not transient, but does not exist in the database yet; uses its id<br/>
        ///     Updates is not transient, exists in db is different from what is in the db.
        /// </summary>
        /// <param name="document">
        ///     Mandatory
        /// </param>
        void SaveOrUpdate(TDoc document);

        void SaveOrUpdate(params TDoc[] docs);

        /// <summary>
        ///     Returns null if not found; exception thrown if found, but conversion failed.
        /// </summary>
        /// <param name="id">
        /// </param>
        /// <returns>
        ///     null if not found
        /// </returns>
        TDoc GetExisting(TKey id);

        /// <summary>
        ///     Remove from storage by primary key.
        /// </summary>
        /// <param name="id">
        ///     Document key
        /// </param>
        /// <returns>
        ///     Null if not found
        /// </returns>
        TDoc Delete(TKey id);

        IEnumerable<TDoc> GetAll();

        int CountAll();
    }
}