// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-01-12
// Comment		
// **********************************************************************************************/

using System;
using System.Linq;
using Pim.CommonLib;

namespace FulltextStorageLib.Util
{
    public static class TaskExtensions
    {
        public static IFactory<ITask> AsFactory(this ITask targetTask)
        {
            return new SingletonFactory<ITask>(targetTask);
        }


        /// <param name="targetTask">
        ///     Mandatory, task to retry
        /// </param>
        /// <param name="retryCount">
        ///     Must be greater than 0; max number of execution attempts
        /// </param>
        /// <param name="retryDelayMilliseconds">
        ///     Number of milliseconds to wait before retrying; must be 0..10000 inclusive
        /// </param>
        /// <param name="targetFailedTask">
        ///     Optional, task to execute after every failed attempt of <paramref name="targetTask"/>
        /// </param>
        public static RetryTask<TE> Retry<TE>(this ITask targetTask, int retryCount, int retryDelayMilliseconds = 0, ITask targetFailedTask = null)
            where TE : Exception
        {
            Check.DoCheckArgument(retryDelayMilliseconds >= 0 && retryDelayMilliseconds <= 10000, "Retry delay must be between 0 and 10 seconds inclusive");

            return new RetryTask<TE>(targetTask, retryCount, targetFailedTask, retryDelayMilliseconds);
        }

        /// <summary>
        ///     Create task executing sequence of other tasks. Execution will stop when any of them throws exception.
        /// </summary>
        /// <param name="targetTask">
        ///     First task to execute.
        /// </param>
        /// <param name="followingTasks">
        ///     Arbitrary number of tasks to execute next in sequence.
        /// </param>
        /// <returns>
        ///     New task
        /// </returns>
        public static CompositeTask Composite(this ITask targetTask, params ITask[] followingTasks)
        {
            Check.DoRequireArgumentNotNull(targetTask, nameof(targetTask));
            var allTasks = Enumerable.Repeat(targetTask, 1).Concat(followingTasks).ToList();
            return new CompositeTask(allTasks, null);
        }

        /// <summary>
        ///     Create task which will evaluate predicate and execute wrapped task if result is true.
        ///     To add 'else' branch use returned <see cref="ConditionalTask"/>'s method (<see cref="ConditionalTask.Else"/>).
        /// </summary>
        /// <param name="thenTask">
        ///     Mandatory, task to execute is <paramref name="predicate"/> evaluated to true
        /// </param>
        /// <param name="predicate">
        ///     Predicate
        /// </param>
        /// <param name="conditionName">
        ///     Optional, name of the predicate or condition
        /// </param>
        /// <returns>
        ///     New task
        /// </returns>
        public static ConditionalTask If(this ITask thenTask, Func<bool> predicate, string conditionName)
        {
            Check.DoRequireArgumentNotNull(predicate, nameof(predicate));

            return new ConditionalTask(predicate, thenTask, null, conditionName);
        }

        /// <summary>
        ///     Create task executing <paramref name="targetTask"/> in try-catch block. Exception handlers and 'finally' task can be added incrementally,
        ///     see <see cref="TryTask.Catch{TE}"/>, <see cref="TryTask.Finally"/>.
        ///     All exceptions will be logged and re-thrown; to catch and suppress exception or add 'finally' task use chained <see cref="TryTask"/>'s methods.
        /// </summary>
        /// <param name="targetTask">
        ///     Mandatory
        /// </param>
        /// <returns>
        ///     New task which executes <paramref name="targetTask"/> in try-catch block;
        /// </returns>
        public static TryTask Try(this ITask targetTask)
        {
            return new TryTask(targetTask, null);
        }

        /// <summary>
        ///     Convert parameterless action to named task.
        /// </summary>
        /// <param name="action">
        ///     Mandatory
        /// </param>
        /// <param name="name">
        ///     Optional
        /// </param>
        public static ActionTask AsTask(this Action action, string name)
        {
            return new ActionTask(action, name);
        }

        public static DelegatingFactory<ITask> AsFactory(this Func<ITask> factoryFunction)
        {
            return new DelegatingFactory<ITask>(factoryFunction);
        }
    }
}