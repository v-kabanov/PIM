// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-01-30
// Comment		
// **********************************************************************************************/

using System;
using Pim.CommonLib;

namespace FulltextStorageLib.Util
{
    /// <summary>
    ///     Task representing named action.
    /// </summary>
    public class ActionTask : ITask
    {
        public Action Action { get; }

        /// <param name="action">
        ///     Mandatory
        /// </param>
        /// <param name="name">
        ///     Optional
        /// </param>
        public ActionTask(Action action, string name)
        {
            Check.DoRequireArgumentNotNull(action, nameof(action));

            Action = action;
            Name = name ?? GetType().Name;
        }

        public void Execute()
        {

            Action.Invoke();
        }

        public string Name { get; }
    }
}