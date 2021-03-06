﻿// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FulltextStorageLib.Util
{

    /// <summary>
    /// Provides a task scheduler that ensures a maximum concurrency level While
    /// running on top of the ThreadPool.
    /// </summary>
    /// <remarks>Taken with no functional changes from code provided on MSDN http://msdn.microsoft.com/en-us/library/ee789351.aspx </remarks>
    public class LimitedConcurrencyScheduler : TaskScheduler
    {
        /// <summary>Whether the current thread is processing work items.</summary>
        [ThreadStatic()]
        private static bool _currentThreadIsProcessingItems;

        /// <summary>The maximum concurrency (number of threads) allowed by this scheduler.</summary>
        private readonly int _maxDegreeOfParallelism;

        /// <summary>The list of tasks to be executed.</summary>
        // protected by lock(_tasks)
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();

        /// <summary>Whether the scheduler is currently processing work items.</summary>
        // protected by lock(_tasks)
        private int _delegatesQueuedOrRunning;

        /// <summary>
        /// Initializes an instance of the LimitedConcurrencyScheduler class with the
        /// specified degree of parallelism.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism provided by this scheduler.</param>
        public LimitedConcurrencyScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }
            _maxDegreeOfParallelism = maxDegreeOfParallelism;

        }

        /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
        public override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;

        /// <summary>Queues a task to the scheduler.</summary>
        /// <param name="task">The task to be queued.</param>
        protected override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.

            lock (_tasks)
            {
                _tasks.AddLast(task);

                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    _delegatesQueuedOrRunning = _delegatesQueuedOrRunning + 1;
                    NotifyThreadPoolOfPendingWork();
                }
            }

        }

        /// <summary>
        /// Informs the ThreadPool that there's work to be executed for this scheduler.
        /// </summary>
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem((stateInfo) => 
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;

                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                _delegatesQueuedOrRunning = _delegatesQueuedOrRunning - 1;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);

                    }
                    // We're done processing items on the current thread
                }
                finally
                {
                    _currentThreadIsProcessingItems = false;
                }
            }, null);
        }


        /// <summary>Attempts to execute the specified task on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued"></param>
        /// <returns>Whether the task could be executed on the current thread.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems)
            {
                return false;
            }

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
            {
                TryDequeue(task);
            }
            // Try to run the task.
            return base.TryExecuteTask(task);
        }


        /// <summary>Attempts to remove a previously scheduled task from the scheduler.</summary>
        /// <param name="t">The task to be removed.</param>
        /// <returns>Whether the task could be found and removed.</returns>
        protected override bool TryDequeue(Task t)
        {
            lock (_tasks)
            {
                return _tasks.Remove(t);
            }
        }

        /// <summary>Gets an enumerable of the tasks currently scheduled on this scheduler.</summary>
        /// <returns>An enumerable of the tasks currently scheduled.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;

            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken)
                {
                    return _tasks.ToArray();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_tasks);
                }
            }
        }
    }

}
