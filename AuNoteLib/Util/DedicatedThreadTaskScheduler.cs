// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace FulltextStorageLib.Util
{

    /// <summary>
    /// The schedule executes tasks sequentially on a single thread.
    /// </summary>
    /// <remarks>
    /// Taken from http://msdn.microsoft.com/en-us/library/dd997413%28VS.100%29.aspx
    /// </remarks>
    public sealed class DedicatedThreadTaskScheduler : TaskScheduler, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private BlockingCollection<Task> _tasks = new BlockingCollection<Task>();

        private Thread _thread;
        public DedicatedThreadTaskScheduler()
		{
			_thread = new Thread(() =>
			{
			    foreach (var task in _tasks.GetConsumingEnumerable()) {
			        try
			        {
			            TryExecuteTask(task);
			        }
			        catch (InvalidOperationException ex)
			        {
			            // task is not associated with this scheduler
			            Log.Fatal("Unexpected", ex);
			        }
			    }
			});
			_thread.IsBackground = true;
			_thread.Start();
		}

        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!ReferenceEquals(Thread.CurrentThread, _thread))
                return false;
            return TryExecuteTask(task);
        }

        public override int MaximumConcurrencyLevel => 1;

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks.ToArray();
        }

        // Dispose is not thread-safe with other members.
        // It may only be used when no more tasks will be queued
        // to the scheduler.  This implementation will block
        // until all previously queued tasks have completed.
        public void Dispose()
        {
            if (_thread != null)
            {
                _tasks.CompleteAdding();
                _thread.Join();
                _tasks.Dispose();
                _tasks = null;
                _thread = null;
            }
        }
    }
}
