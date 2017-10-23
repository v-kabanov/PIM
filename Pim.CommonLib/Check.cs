// Provides support for Design By Contract
// as described by Bertrand Meyer in his seminal book,
// Object-Oriented Software Construction (2nd Ed) Prentice Hall 1997
// (See chapters 11 and 12).
//
// See also Building Bug-free O-O Software: An Introduction to Design by Contract
// http://www.eiffel.com/doc/manuals/technology/contract/
// 
// The following conditional compilation symbols are supported:
// 
// These suggestions are based on Bertrand Meyer's Object-Oriented Software Construction (2nd Ed) p393
// 
// DBC_CHECK_ALL           - Check assertions - implies checking preconditions, postconditions and invariants
// DBC_CHECK_INVARIANT     - Check invariants - implies checking preconditions and postconditions
// DBC_CHECK_POSTCONDITION - Check postconditions - implies checking preconditions 
// DBC_CHECK_PRECONDITION  - Check preconditions only, e.g., in Release build
// 
// A suggested default usage scenario is the following:
// 
// #if DEBUG
// #define DBC_CHECK_ALL    
// #else
// #define DBC_CHECK_PRECONDITION
// #endif
//
// Alternatively, you can define these in the project properties dialog.

using System;
using System.Diagnostics;

namespace Pim.CommonLib
{
    public static class Check
    {
        /// <summary>
        ///		Throws ArgumentNullException if necessary.
        /// </summary>
        public static void DoRequireArgumentNotNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }
        /// <summary>
        ///		Throws ArgumentException if necessary.
        /// </summary>
        public static void DoRequireArgumentNotBlank(string arg, string argName)
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                throw new ArgumentException($"{argName} must not be blank");
            }
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        public static void DoRequire(bool assertion, string message)
        {
            if (!assertion) throw new PreconditionException(message);
        }

        /// <summary>
        ///     Postcondition check.
        /// </summary>
        public static void DoEnsure(bool assertion, string message)
        {
            if (!assertion) throw new PostconditionException(message);
        }

        /// <summary>
        ///     Postcondition check.
        /// </summary>
        public static void DoEnsureLambda(bool assertion, Func<string> message)
        {
            if (!assertion) throw new PostconditionException(message.Invoke());
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        public static void DoAssertLambda(bool assertion, Func<string> message)
        {
            if (!assertion) throw new AssertionException(message.Invoke());
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        public static void DoRequire(bool assertion, string message, Exception inner)
        {
            if (!assertion) throw new PreconditionException(message, inner);
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        public static void DoAssertLambda(bool assertion, Func<string> message, Func<Exception> inner)
        {
            if (!assertion) throw new PreconditionException(message.Invoke(), inner.Invoke());
        }

        /// <summary>
        ///		Guaranteed precondition check.
        /// </summary>
        public static void DoAssertLambda(bool assertion, Func<Exception> getExceptionToThrow)
        {
            if (!assertion) throw getExceptionToThrow.Invoke();
        }

        public static void DoCheckArgument(bool assertion, Func<string> getErrorMessage)
        {
            if (!assertion)
            {
                throw new ArgumentException(getErrorMessage());
            }
        }

        public static void DoCheckArgument(bool assertion, string message = "", string argName = "")
        {
            if (!assertion)
            {
                throw new ArgumentException(message, argName);
            }
        }


        /// <summary>
        ///		Guaranteed check with <see cref="InvalidOperationException"/> thrown if assertion fails
        /// </summary>
        /// <param name="assertion">
        ///		The assertion condition
        /// </param>
        /// <param name="exceptionMessage">
        ///		Message to return from the thrown exception if assertion fails. Pass <see langword="null"/>
        ///		to use default constructor for <see cref="InvalidOperationException"/>
        /// </param>
        public static void DoCheckOperationValid(bool assertion, string exceptionMessage)
        {
            if (assertion) return;

            if (string.IsNullOrEmpty(exceptionMessage))
            {
                throw new InvalidOperationException();
            }

            throw new InvalidOperationException(exceptionMessage);
        }

        /// <summary>
        ///		Guaranteed check with <see cref="InvalidOperationException"/> thrown if assertion fails
        /// </summary>
        /// <param name="assertion">
        ///		The assertion condition
        /// </param>
        /// <param name="getExceptionMessage">
        ///		Delegate retrieving message to return from the thrown exception if assertion fails. Pass <see langword="null"/>
        ///		to use default constructor for <see cref="InvalidOperationException"/>
        /// </param>
        public static void DoCheckOperationValid(bool assertion, Func<string> getExceptionMessage)
        {
            if (assertion) return;

            if (getExceptionMessage == null)
            {
                throw new InvalidOperationException();
            }

            throw new InvalidOperationException(getExceptionMessage());
        }

        /// <summary>
        ///		Guaranteed check with <see cref="InvalidOperationException"/> thrown if assertion fails
        /// </summary>
        /// <param name="assertion">
        ///		The assertion condition
        /// </param>
        /// <remarks>
        ///		The exception if thrown is instantiated using the default constructor
        /// </remarks>
        public static void DoCheckOperationValid(bool assertion)
        {
            DoCheckOperationValid(assertion, (string)null);
        }

        public static void DoCheckInvariant(bool assertion, string exceptionMessage)
        {
            if (assertion) return;

            throw new InvariantException(exceptionMessage);
        }

        public static void DoCheckInvariant(bool assertion, Func<string> exceptionMessageGetter)
        {
            if (assertion) return;

            throw new InvariantException(exceptionMessageGetter?.Invoke());
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        public static void DoRequire(bool assertion)
        {
            if (!assertion) throw new PreconditionException("Precondition failed.");
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT"),
         Conditional("DBC_CHECK_PRECONDITION")]
        [Conditional("DEBUG")]
        public static void RequireArgumentNotNull(object arg, string argName)
        {
            DoRequireArgumentNotNull(arg, argName);
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"), 
         Conditional("DBC_CHECK_INVARIANT"), 
         Conditional("DBC_CHECK_PRECONDITION")]
        [Conditional("DEBUG")]
        public static void Require(bool assertion, string message)
        {
            DoRequire(assertion, message);
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT"),
         Conditional("DBC_CHECK_PRECONDITION")]
        [Conditional("DEBUG")]
        public static void RequireLambda(bool assertion, Func<string> message)
        {
            DoAssertLambda(assertion, message);
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"), 
         Conditional("DBC_CHECK_INVARIANT"), 
         Conditional("DBC_CHECK_PRECONDITION")]
        [Conditional("DEBUG")]
        public static void Require(bool assertion, string message, Exception inner)
        {
            DoRequire(assertion, message, inner);
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT"),
         Conditional("DBC_CHECK_PRECONDITION")]
        [Conditional("DEBUG")]
        public static void RequireLambda(bool assertion, Func<string> message, Func<Exception> inner)
        {
            DoAssertLambda(assertion, message, inner);
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT"),
         Conditional("DBC_CHECK_PRECONDITION")]
        [Conditional("DEBUG")]
        public static void RequireLambda(bool assertion, Func<Exception> getExceptionToThrow)
        {
            DoAssertLambda(assertion, getExceptionToThrow);
        }

        /// <summary>
        ///     Precondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT"), 
         Conditional("DBC_CHECK_PRECONDITION")]
        [Conditional("DEBUG")]
        public static void Require(bool assertion)
        {
            DoRequire(assertion);
        }

        /// <summary>
        ///     Postcondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT"), 
         Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void Ensure(bool assertion, string message)
        {
            if (!assertion) throw new PostconditionException(message);
        }

        /// <summary>
        ///     Postcondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT"),
         Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void EnsureLambda(bool assertion, Func<string> message)
        {
            if (!assertion)
               throw new PostconditionException(message.Invoke());
        }

        /// <summary>
        ///     Postcondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL")]
        [Conditional("DBC_CHECK_INVARIANT")]
        [Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void EnsureLambda(bool assertion, Func<Exception> getExceptionToThrow)
        {
            if (!assertion) throw getExceptionToThrow.Invoke();
        }

        /// <summary>
        ///     Postcondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT"), 
         Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void Ensure(bool assertion, string message, Exception inner)
        {
            if (!assertion) throw new PostconditionException(message, inner);
        }

        /// <summary>
        ///     Postcondition check.
        /// </summary>
        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT"), 
         Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void Ensure(bool assertion)
        {
            if (!assertion) throw new PostconditionException("Postcondition failed.");
        }
		
        [Conditional("DBC_CHECK_ALL")]
        [Conditional("DBC_CHECK_INVARIANT")]
        [Conditional("DEBUG")]
        public static void Invariant(bool assertion, string message)
        {
            if (!assertion)	throw new InvariantException(message);
        }

        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT")]
        [Conditional("DEBUG")]
        public static void Invariant(bool assertion, string message, Exception inner)
        {
            if (!assertion) throw new InvariantException(message, inner);
        }

        [Conditional("DEBUG")]
        public static void Invariant(bool assertion, Func<Exception> getException)
        {
            if (!assertion)
            {
                throw getException();
            }
        }

        [Conditional("DBC_CHECK_ALL"),
         Conditional("DBC_CHECK_INVARIANT")]
        [Conditional("DEBUG")]
        public static void Invariant(bool assertion)
        {
            if (!assertion) throw new InvariantException("Invariant failed.");
        }

        [Conditional("DBC_CHECK_ALL")]
        [Conditional("DEBUG")]
        public static void Assert(bool assertion, string message)
        {
            if (!assertion) throw new AssertionException(message);
        }

        [Conditional("DBC_CHECK_ALL")]
        [Conditional("DEBUG")]
        public static void Assert(bool assertion, string message, Exception inner)
        {
            if (!assertion) throw new AssertionException(message, inner);
        }

        [Conditional("DBC_CHECK_ALL")]
        [Conditional("DEBUG")]
        public static void Assert(bool assertion)
        {
            if (!assertion) throw new AssertionException("Assertion failed.");
        }
    }
}