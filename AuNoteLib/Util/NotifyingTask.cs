// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System;

namespace FulltextStorageLib.Util
{
    /// <summary>
    ///     Adds "Executed" event to an existing task
    /// </summary>
    public class NotifyingTask : ITask
    {
        private ITask _target;

        public NotifyingTask(ITask target)
        {
            Check.DoRequireArgumentNotNull(target, "target");

            _target = target;
        }

        /// <summary>
        /// Event raised imediately after executing target task, regardless of whether exception has been thrown.
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="args">
        /// Will contain reference to target task (rather than this wrapper)
        /// </param>
        /// <remarks></remarks>
        public event ExecutedEventHandler Executed;

        public delegate void ExecutedEventHandler(object sender, TaskExecutedEventArgs args);

        public void Execute()
        {
            try
            {
                _target.Execute();
                OnExecuted(null);
            }
            catch (Exception ex)
            {
                OnExecuted(ex);
                throw;
            }

        }

        public string Name => _target.Name;

        protected virtual void OnExecuted(Exception exception)
        {
            Executed?.Invoke(this, new TaskExecutedEventArgs(exception, _target));
        }
    }
}