// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace AuNoteLib.Util
{
    /// <summary>
    ///     Executes a sequence of tasks, stops after failure (does not catch exceptions).
    /// </summary>
    /// <remarks>
    ///     Tasks are executed in the same order as they are added.
    ///     If task fails (with exception), remaining tasks will not be executed.
    /// </remarks>
    public class CompositeTask : ITask
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<ITask> _tasks;

        /// <summary>
        ///     Initialise with a collection of tasks
        /// </summary>
        /// <param name="tasks">
        ///     The tasks to execute; sequence is preserved
        /// </param>
        /// <param name="name">
        ///     Optional
        /// </param>
        public CompositeTask(IEnumerable<ITask> tasks, string name)
        {
            Check.DoRequireArgumentNotNull(tasks, nameof(tasks));

            _tasks = new List<ITask>(tasks);
            Name = !string.IsNullOrWhiteSpace(name) ? name : $"Composite({string.Join(", ", _tasks.Select(t => t.Name))})";
        }

        /// <summary>
        /// Add another inner task to the end of the list
        /// </summary>
        /// <param name="task">
        /// Must not be null
        /// </param>
        /// <remarks></remarks>
        public void Add(ITask task)
        {
            Check.DoRequireArgumentNotNull(task, "task");

            _tasks.Add(task);
        }

        public void Execute()
        {
            Log.InfoFormat("Executing {0}", Name);
            foreach (var task in _tasks)
            {
                Log.InfoFormat("{0} executing {1}", Name, task.Name);
                task.Execute();
            }
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///     Convenience factory method.
        /// </summary>
        public static CompositeTask Create(string name, params ITask[] tasks)
        {
            Check.DoRequireArgumentNotNull(name, "name");
            Check.DoRequireArgumentNotNull(tasks, "tasks");
            Check.DoCheckArgument(tasks.Length > 0, "Task list is empty");

            return new CompositeTask(tasks, name);
        }
    }
}
