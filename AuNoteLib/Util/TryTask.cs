// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-01-12
// Comment		
// **********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace AuNoteLib.Util
{
    /// <summary>
    ///     Task executing one task and then another (try - finally block) even if first one throws exception.
    ///     At minimum it just logs exception message at INFO level and full exception at DEBUG level, so can be used as logging wrapper.
    /// </summary>
    public class TryTask : ITask
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IList<KeyValuePair<Type, ITask>> _exceptionHandlers;

        /// <summary>
        ///     Mandatory, task to try
        /// </summary>
        public ITask TargetTask { get; }

        /// <summary>
        ///     Optional, task to execute after <see cref="TargetTask"/> in 'finally' block, even if target task throws exception
        /// </summary>
        public ITask FinallyTask { get; private set; }
        
        /// <summary>
        ///     Initialize with default name composed from underlying tasks.
        /// </summary>
        /// <param name="targetTask">
        ///     Mandatory, task to try
        /// </param>
        /// <param name="finallyTask">
        ///     Mandatory, task to execute after <paramref name="targetTask"/> in 'finally' block, even if target task throws exception
        /// </param>
        public TryTask(ITask targetTask, ITask finallyTask)
            : this (targetTask, finallyTask, null)
        {
        }

        /// <summary>
        ///     Initialize with given name
        /// </summary>
        /// <param name="targetTask">
        ///     Mandatory, task to try
        /// </param>
        /// <param name="finallyTask">
        ///     Optional, task to execute after <paramref name="targetTask"/> in 'finally' block, even if target task throws exception
        /// </param>
        /// <param name="name">
        ///     Optional
        /// </param>
        public TryTask(ITask targetTask, ITask finallyTask, string name)
        {
            Check.DoRequireArgumentNotNull(targetTask, nameof(targetTask));

            TargetTask = targetTask;
            FinallyTask = finallyTask;
            Name = !string.IsNullOrWhiteSpace(name) ? name : $"try ({targetTask.Name})";

            _exceptionHandlers = new List<KeyValuePair<Type, ITask>>();
        }

        public void Execute()
        {
            try
            {
                TargetTask.Execute();
            }
            catch(Exception exception)
            {
                Log.InfoFormat("{0} caught exception {1}", Name, exception.Message);
                Log.Debug("Exception", exception);

                var handlerPair = _exceptionHandlers.Cast<KeyValuePair<Type, ITask>?>()
                    .FirstOrDefault(p => p.Value.Key.IsInstanceOfType(exception));

                if (handlerPair.HasValue)
                {
                    var handlerPairValue = handlerPair.Value;

                    Log.InfoFormat("{0} handler is {1}", handlerPairValue.Key.Name, handlerPairValue.Value?.Name ?? "none");

                    (handlerPairValue.Value as IExceptionHandlerTask)?.SetSource(TargetTask, exception);

                    handlerPairValue.Value?.Execute();
                }
                else
                {
                    Log.Info("No handler, rethrowing");
                    throw;
                }
            }
            finally
            {
                FinallyTask?.Execute();
            }
        }

        /// <summary>
        ///     Add exception handler. Unhandled exceptions pass through. Order of addition is significant, unreachable handler addition will fail.
        /// </summary>
        /// <typeparam name="TE">
        ///     Type of exception to catch
        /// </typeparam>
        /// <param name="exceptionHandler">
        ///     Optional, task to execute when exception of type <typeparamref name="TE"/> is caught.
        ///     If not provided, exception is still caught and not re-thrown.
        ///     If <paramref name="exceptionHandler"/> implements <see cref="IExceptionHandlerTask"/>, its <see cref="IExceptionHandlerTask.SetSource"/>
        ///     will be invoked before execution.
        /// </param>
        /// <returns>
        ///     Itself for chaining.
        /// </returns>
        public TryTask Catch<TE>(ITask exceptionHandler)
            where TE : Exception
        {
            Check.DoCheckOperationValid(!_exceptionHandlers.Any(p => p.Key.IsAssignableFrom(typeof(TE))), "Handler unreachable, already handled");

            _exceptionHandlers.Add(new KeyValuePair<Type, ITask>(typeof(TE), exceptionHandler));

            return this;
        }

        /// <summary>
        ///     Add 'finally block'
        /// </summary>
        /// <param name="task">
        ///     Mandatory, task to execute in a 'finally' block after <see cref="TargetTask"/> and exception handler.
        /// </param>
        /// <returns></returns>
        public TryTask Finally(ITask task)
        {
            Check.DoRequireArgumentNotNull(task, nameof(task));
            Check.DoCheckOperationValid(FinallyTask == null, "Already set");

            FinallyTask = task;

            return this;
        }

        public string Name { get; }
    }
}