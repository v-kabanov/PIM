// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace AuNoteLib.Util
{
    /// <summary>
    ///     Executes tasks asynchronously in the given scheduler.
    /// </summary>
    public class TaskExecutor : ITaskExecutor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public TaskFactory TaskFactory { get; private set; }

        public TaskScheduler TaskScheduler { get; }

        private readonly IDictionary<Task, byte> _pendingTasks;

        private readonly object _syncRoot = new object();

        /// <summary>
        ///     Read-only flag which is initially true and can be irreversibly set to false by <see cref="StopAcceptingNewTasks"/>
        /// </summary>
        public bool AcceptingNewTasks { get; private set; }

        /// <summary>
        ///     Create instance executing tasks in a single dedicated thread
        /// </summary>
        public TaskExecutor()
            : this(new DedicatedThreadTaskScheduler())
        {
        }

        /// <summary>
        ///     Create instance with the specified scheduler
        /// </summary>
        public TaskExecutor(TaskScheduler scheduler)
        {
            Check.DoRequireArgumentNotNull(scheduler, nameof(scheduler));

            TaskFactory = new TaskFactory(scheduler);
            TaskScheduler = scheduler;
            _pendingTasks = new ConcurrentDictionary<Task, byte>();
            AcceptingNewTasks = true;
        }

        public Task Schedule(Action action)
        {
            Check.DoRequireArgumentNotNull(action, "action");
            CheckAcceptingNewTasks();

            Task result;

            lock (_syncRoot)
            {
                result = TaskFactory.StartNew(action);

                _pendingTasks.Add(result, 0);
            }

            result.ContinueWith(OnTaskCompleted);

            return result;
        }

        public StatefulTask Schedule(ITask task)
        {
            Check.DoRequireArgumentNotNull(task, nameof(task));
            CheckAcceptingNewTasks();
            var wrapper = new StatefulTask(task);

            lock (_syncRoot)
            {
                wrapper.Start(TaskFactory.Scheduler);

                _pendingTasks.Add(wrapper, 0);
            }

            wrapper.ContinueWith(OnTaskCompleted);

            return wrapper;
        }

        /// <summary>
        ///     One-way prohibition; no way to resume.
        /// </summary>
        public void StopAcceptingNewTasks()
        {
            AcceptingNewTasks = false;
        }

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
        public bool WaitForAllCurrentTasksToFinish(int maxWaitMilliseconds)
        {
            Check.DoCheckArgument(maxWaitMilliseconds == -1 || maxWaitMilliseconds > 0, "Timeout must be 0, positive or -1");
            CheckNotDisposed();

            var startTime = DateTime.Now;
            var result = false;

            if (Monitor.TryEnter(_syncRoot, maxWaitMilliseconds))
            {
                try
                {
                    var millisecondsTimeout = -1;
                    do
                    {
                        if (maxWaitMilliseconds > 0)
                        {
                            millisecondsTimeout = maxWaitMilliseconds - (int)(DateTime.Now - startTime).TotalMilliseconds;
                            if (millisecondsTimeout <= 0)
                                break;
                        }

                        var tasks = _pendingTasks.Keys.ToArray();
                        if (tasks.Length == 0)
                        {
                            return true;
                        }

                        try
                        {
                            result = Task.WaitAll(tasks, millisecondsTimeout);
                        }
                        catch (ObjectDisposedException exception)
                        {
                            Log.Debug("Exception waiting for task completion", exception);
                            Log.Error("One of the tasks being awaited got disposed");
                        }
                        catch (AggregateException exception)
                        {
                            Log.Debug("Exception waiting for task completion", exception);
                            // based on the decompiled code it can only happen when tasks completed but some of them were cancelled or finished with exception
                            Log.WarnFormat("Some of the tasks were cancelled or threw exception: {0}", exception.Message);
                            result = true;
                        }
                    }
                    while (!result);
                }
                finally
                {
                    Monitor.Exit(_syncRoot);
                }
            }
            return result;
        }
        private void OnTaskCompleted(Task task)
        {
            _pendingTasks.Remove(task);
        }

        /// <summary>
        ///     Get list of scheduled and not yet finished stateful tasks scheduled with <see cref="Schedule(ITask)"/>
        /// </summary>
        public IList<StatefulTask> GetPendingStatefulTasks()
        {
            CheckNotDisposed();

            return _pendingTasks.Keys.Where(t => t is StatefulTask).Cast<StatefulTask>().ToList();
        }

        private void CheckNotDisposed()
        {
            Check.DoAssertLambda(!_disposedValue, () => new ObjectDisposedException(GetType().Name));
        }

        private void CheckAcceptingNewTasks()
        {
            CheckNotDisposed();
            Check.DoCheckOperationValid(AcceptingNewTasks, "Executor is not accepting new tasks");
        }

        // To detect redundant calls
        private bool _disposedValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing">
        /// whether the method is called from IDisposable.Dispose(); the alternative caller is Finalize() which must
        /// supply False, in which case you must not access managed members
        /// </param>
        /// <remarks></remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                StopAcceptingNewTasks();

                if (disposing)
                {
                    // dispose managed state (managed objects).
                    var disposable = TaskFactory.Scheduler as IDisposable;
                    disposable?.Dispose();

                    TaskFactory = null;
                }
                // free unmanaged resources (unmanaged objects) and override Finalize() below.
                // must not access managed members here
                // set large fields to null.
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}