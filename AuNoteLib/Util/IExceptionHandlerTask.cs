// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-01-24
// Comment		
// **********************************************************************************************/

using System;

namespace FulltextStorageLib.Util
{
    /// <summary>
    ///     Task that is handling exception thrown from another task
    /// </summary>
    public interface IExceptionHandlerTask : ITask
    {
        /// <summary>
        ///     Exception to be set by try-catch task before executing this handler task
        /// </summary>
        void SetSource(ITask sourceTask, Exception exception);
    }
}