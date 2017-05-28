// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-04
// Comment		
// **********************************************************************************************/

using System;

namespace FulltextStorageLib
{
    public interface IFulltextIndexEntry
    {
        /// <summary>
        ///     This is fulltext key.
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     This will appear in search results.
        /// </summary>
        string Name { get; }
    }

    public interface INoteHeader : IFulltextIndexEntry
    {
        DateTime CreateTime { get; }
    }
}