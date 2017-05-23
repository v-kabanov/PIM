// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System;
using System.Threading.Tasks;

namespace AuNoteLib.Util
{
    /// <summary>
    ///     Wraps POCO task object in a <see cref="Task"/> so that framework's infrastructure can work with it (e.g. <see cref="TaskScheduler"/>).
    /// </summary>
    public class StatefulTask : Task, ITask
    {
        private static readonly Action<object> Action = task => { ((ITask)task).Execute(); };

        /// <param name="task">
        ///     Mandatory
        /// </param>
        public StatefulTask(ITask task)
            : base(Action, task)
        {
            Check.DoRequireArgumentNotNull(task, nameof(task));

            TargetTask = task;
        }

        public string Name => TargetTask.Name;

        public void Execute()
        {
            TargetTask.Execute();
        }

        public ITask TargetTask { get; }
    }
}