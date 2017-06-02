// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-02
// Comment  
// **********************************************************************************************/
// 
using System;

namespace FulltextStorageLib
{
    public static class Extensions
    {
        /// <summary>
        ///     Get value of one of searchable note time properties.
        /// </summary>
        /// <param name="note">
        ///     Mandatory
        /// </param>
        /// <param name="searchableDocumentTime">
        ///     Identifies property.
        /// </param>
        /// <returns>
        ///     Time property value.
        /// </returns>
        public static DateTime GetSeachableTime(this INoteHeader note, SearchableDocumentTime searchableDocumentTime)
        {
            return searchableDocumentTime == SearchableDocumentTime.Creation
                ? note.CreateTime
                : note.LastUpdateTime;
        }

        /// <summary>
        ///     Truncate datetime to seconds, remove all fractions.
        /// </summary>
        public static DateTime TrimToSecondsPrecision(this DateTime time)
        {
            return time.AddTicks(-time.Ticks % TimeSpan.TicksPerSecond);
        }
    }
}