// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System;

namespace FulltextStorageLib.Util
{
    public class TaskExecutedEventArgs : EventArgs
    {
        private Exception _exception;

        private ITask _task;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">
        /// Nothing means task succeeded
        /// </param>
        /// <param name="task">
        /// Must not be Nothing
        /// </param>
        /// <remarks></remarks>
        public TaskExecutedEventArgs(Exception exception, ITask task)
        {
            Check.DoRequireArgumentNotNull(task, "task");

            _task = task;
            _exception = exception;
        }

        public Exception Exception => _exception;

        /// <summary>
        /// Whether task executed without exceptions.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If this property returns False, <see cref="Exception" /> will return the thrown exception.
        /// </remarks>
        public bool Succeeded => _exception == null;

        public ITask Task => _task;
    }
}