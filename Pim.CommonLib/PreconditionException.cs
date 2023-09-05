using System;

namespace Pim.CommonLib;

/// <summary>
/// Exception raised when a precondition fails.
/// </summary>
[Serializable]
public class PreconditionException : DesignByContractException
{
    /// <summary>
    /// Precondition Exception.
    /// </summary>
    public PreconditionException() {}
    /// <summary>
    /// Precondition Exception.
    /// </summary>
    public PreconditionException(string message) : base(message) {}
    /// <summary>
    /// Precondition Exception.
    /// </summary>
    public PreconditionException(string message, Exception inner) : base(message, inner) {}
}