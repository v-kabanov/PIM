// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

using System;
using System.Reflection;
using log4net;
using Pim.CommonLib;

namespace FulltextStorageLib.Util;

public class ConditionalTask : ITask
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Func<bool> _predicate;

    public ITask ThenTask { get; }

    public ITask ElseTask { get; private set; }

    private readonly string _name;

    /// <param name="predicate">
    ///     Mandatory
    /// </param>
    /// <param name="thenTask">
    ///     Mandatory, task to execute if <paramref name="predicate"/> evaluates to true
    /// </param>
    /// <param name="elseTask">
    ///     Optional, task to execute if <paramref name="predicate"/> evaluates to true
    /// </param>
    /// <param name="conditionName">
    ///     Predicate or condition name, not task name
    /// </param>
    public ConditionalTask(Func<bool> predicate, ITask thenTask, ITask elseTask, string conditionName)
    {
        Check.DoRequireArgumentNotNull(predicate, "predicate");
        Check.DoRequireArgumentNotNull(thenTask, "task");

        _predicate = predicate;
        ThenTask = thenTask;
        ElseTask = elseTask;
        _name = conditionName ?? "predicate";
    }

    public void Execute()
    {
        if (_predicate())
        {
            Log.DebugFormat("{0} executing 'then' task", Name);
            ThenTask.Execute();
        }
        else
        {
            Log.DebugFormat("{0} skipping execution of 'then' task", Name);
            if (ElseTask != null)
            {
                Log.DebugFormat("{0} executing 'else' task", Name);
                ElseTask.Execute();
            }
        }
    }

    public ConditionalTask Else(ITask elseTask)
    {
        Check.DoRequireArgumentNotNull(elseTask, nameof(elseTask));
        Check.DoCheckOperationValid(ElseTask == null, "Else task already set");

        ElseTask = elseTask;
        return this;
    }

    public string Name => $"If {_name} then ({ThenTask.Name}) else ({ElseTask.Name})";
}