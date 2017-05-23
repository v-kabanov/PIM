// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuNoteLib.Util
{
    /// <summary>
    ///     Executes tasks asynchronously in the given scheduler.
    /// </summary>
    public interface ITaskExecutor : IDisposable
    {
        TaskScheduler TaskScheduler { get; }

        Task Schedule(Action action);

        StatefulTask Schedule(ITask task);

        /// <summary>
        ///     Read-only flag which is initially true and can be irreversibly set to false by <see cref="StopAcceptingNewTasks"/>
        /// </summary>
        bool AcceptingNewTasks { get; }

        /// <summary>
        ///     One-way prohibition; no way to resume.
        /// </summary>
        void StopAcceptingNewTasks();

        /// <summary>
        ///     Block scheduling of new tasks and wait until already scheduled ones finish.
        /// </summary>
        /// <param name="maxWaitMilliseconds">
        ///     Maximum wait time in milliseconds; must be 0, positive or -1 for infinite.
        ///     When 0, method effectively checks if there are pending tasks.
        /// </param>
        /// <returns>
        ///     true if all tasks finished before returning from this method
        ///     false if timeout expired before all tasks have finished
        /// </returns>
        /// <remarks>
        ///     This method should be called from one controller thread. If called from multiple threads concurrently only one of them will
        ///     actually be waiting on Tasks and then when that thread enters and next caller blocks new task scheduling some more tasks
        ///     can be scheduled which could lead to timeout.
        /// </remarks>
        bool WaitForAllCurrentTasksToFinish(int maxWaitMilliseconds);

        /// <summary>
        ///     Get list of scheduled and not yet finished stateful tasks scheduled with <see cref="Schedule(ITask)"/>
        /// </summary>
        IList<StatefulTask> GetPendingStatefulTasks();
    }
}