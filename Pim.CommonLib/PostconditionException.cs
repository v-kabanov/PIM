using System;

namespace Pim.CommonLib
{
    /// <summary>
    /// Exception raised when a postcondition fails.
    /// </summary>
    [Serializable]
    public class PostconditionException : DesignByContractException
    {
        /// <summary>
        /// Postcondition Exception.
        /// </summary>
        public PostconditionException() {}
        /// <summary>
        /// Postcondition Exception.
        /// </summary>
        public PostconditionException(string message) : base(message) {}
        /// <summary>
        /// Postcondition Exception.
        /// </summary>
        public PostconditionException(string message, Exception inner) : base(message, inner) {}
    }
}