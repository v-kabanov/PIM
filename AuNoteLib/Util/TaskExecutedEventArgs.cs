// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System;

namespace AuNoteLib.Util
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

        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>
        /// Whether task executed without exceptions.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If this property returns False, <see cref="Exception" /> will return the thrown exception.
        /// </remarks>
        public bool Succeeded
        {
            get { return _exception == null; }
        }

        public ITask Task
        {
            get { return _task; }
        }
    }
}