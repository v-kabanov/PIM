// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-01-09
// Comment		
// **********************************************************************************************/

using System;
using System.Reflection;
using System.Threading;
using log4net;

namespace AuNoteLib.Util
{
    /// <summary>
    ///     Wrap task so that it is retried up to a number of times if <typeparamref name="TE"/> exception is thrown when it is executed.
    /// </summary>
    /// <typeparam name="TE">
    ///     Type of exception to cause retry
    /// </typeparam>
    public class RetryTask<TE> : ITask
        where TE: Exception
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ITask TargetTask { get; }

        /// <summary>
        ///     Task to execute after every failed attempt of <see cref="TargetTask"/>
        /// </summary>
        public ITask TargetFailedTask { get; }

        public int RetryCount { get; }

        /// <summary>
        ///     Number of milliseconds to wait before retrying; must be 0..10000 inclusive
        /// </summary>
        public int RetryDelayMilliseconds { get; }

        /// <param name="targetTask">
        ///     Mandatory, task to retry
        /// </param>
        /// <param name="retryCount">
        ///     Must be greater than 0; max number of execution attempts
        /// </param>
        /// <param name="targetFailedTask">
        ///     Optional, task to execute after every failed attempt of <paramref name="targetTask"/>
        /// </param>
        /// <param name="retryDelayMilliseconds">
        ///     Number of milliseconds to wait before retrying; must be 0..10000 inclusive
        /// </param>
        public RetryTask(ITask targetTask, int retryCount, ITask targetFailedTask, int retryDelayMilliseconds)
        {
            Check.DoRequireArgumentNotNull(targetTask, nameof(targetTask));
            Check.DoCheckArgument(retryCount > 0, "Retry count must be greater than 1");
            Check.DoCheckArgument(retryDelayMilliseconds >= 0 && retryDelayMilliseconds <= 10000, "Retry delay must be between 0 and 10 seconds inclusive");

            TargetTask = targetTask;
            RetryCount = retryCount;
            TargetFailedTask = targetFailedTask;
            RetryDelayMilliseconds = retryDelayMilliseconds;
        }

        public void Execute()
        {
            var success = false;
            var attemptIndex = 0;

            do
            {
                ++attemptIndex;

                try
                {
                    if (attemptIndex > 1 && TargetFailedTask != null)
                    {
                        Log.DebugFormat("Executing failure response task {0}", TargetFailedTask.Name);
                        TargetFailedTask.Execute();
                    }

                    TargetTask.Execute();
                    success = true;
                }
                catch (TE exception)
                {
                    Log.ErrorFormat("Attempt #{0} to execute {1} failed: {2}", attemptIndex, TargetTask.Name, exception.Message);
                    Log.Debug("Exception", exception);

                    if (attemptIndex >= RetryCount)
                    {
                        Log.ErrorFormat("Reached limit of {0} retries of {1}", RetryCount, TargetTask.Name);
                        throw;
                    }

                    if (RetryDelayMilliseconds > 0)
                    {
                        Log.InfoFormat("Waiting {0} milliseconds before retrying", RetryDelayMilliseconds);
                        Thread.Sleep(RetryDelayMilliseconds);
                    }
                }
            }
            while (!success);
        }

        public string Name => $"Retry({TargetTask.Name}, {RetryCount})";
    }
}