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
using System.Linq.Expressions;

using log4net;

namespace AuNoteLib.Util
{
	/// <summary>
	/// Design By Contract Checks.
	/// 
	/// Each method generates an exception or
	/// a trace assertion statement if the contract is broken.
	/// </summary>
	/// <remarks>
	/// This example shows how to call the Require method.
	/// Assume DBC_CHECK_PRECONDITION is defined.
	/// <code>
	/// public void Test(int x)
	/// {
	/// 	try
	/// 	{
	///			Check.Require(x > 1, "x must be > 1");
	///		}
	///		catch (System.Exception ex)
	///		{
	///			Console.WriteLine(ex.ToString());
	///		}
	///	}
	/// </code>
	/// If you wish to use trace assertion statements, intended for Debug scenarios,
	/// rather than exception handling then set 
	/// 
	/// <code>Check.UseAssertions = true</code>
	/// 
	/// You can specify this in your application entry point and maybe make it
	/// dependent on conditional compilation flags or configuration file settings, e.g.,
	/// <code>
	/// #if DBC_USE_ASSERTIONS
	/// Check.UseAssertions = true;
	/// #endif
	/// </code>
	/// You can direct output to a Trace listener. For example, you could insert
	/// <code>
	/// Trace.Listeners.Clear();
	/// Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
	/// </code>
	/// 
	/// or direct output to a file or the Event Log.
	/// 
	/// (Note: For ASP.NET clients use the Listeners collection
	/// of the Debug, not the Trace, object and, for a Release build, only exception-handling
	/// is possible.)
	/// </remarks>
	/// 
	public static class Check
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Check).Name);
		#region Interface


		#region compulsory checks

		/// <summary>
		///		Throws ArgumentNullException if necessary.
		/// </summary>
		public static void DoRequireArgumentNotNull(object arg, string argName)
		{
			if (UseExceptions)
			{
				if (arg == null)
				{
					throw new ArgumentNullException(argName);
				}
			}
			else
			{
				TraceAssertionImpl(arg != null, string.Format("Precondition: {0} is not null", argName));
			}
		}
        /// <summary>
        ///		Throws ArgumentException if necessary.
        /// </summary>
        public static void DoRequireArgumentNotBlank(string arg, string argName)
        {
            if (UseExceptions)
            {
                if (string.IsNullOrWhiteSpace(arg))
                {
                    throw new ArgumentException($"{argName} must not be blank");
                }
            }
            else
            {
                TraceAssertionImpl(arg != null, string.Format("Precondition: {0} is not blank", argName));
            }
        }

        /// <summary>
        /// Precondition check.
        /// </summary>
        public static void DoRequire(bool assertion, string message)
		{
			if (UseExceptions)
			{
				if (!assertion) throw new PreconditionException(message);
			}
			else
			{
				TraceAssertionImpl(assertion, "Precondition: " + message);
			}
		}

		/// <summary>
		/// Postcondition check.
		/// </summary>
		public static void DoEnsure(bool assertion, string message)
		{
			if (UseExceptions)
			{
				if (!assertion) throw new PostconditionException(message);
			}
			else
			{
				TraceAssertionImpl(assertion, "Postcondition: " + message);
			}
		}

        /// <summary>
        /// Postcondition check.
        /// </summary>
        public static void DoEnsureLambda(bool assertion, Func<string> message)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new PostconditionException(message.Invoke());
            }
            else
            {
                TraceAssertionImpl(assertion, "Postcondition: " + message.Invoke());
            }
        }

        /// <summary>
        /// Precondition check.
        /// </summary>
        public static void DoAssertLambda(bool assertion, Func<string> message)
		{
			if (UseExceptions)
			{
				if (!assertion) throw new AssertionException(message.Invoke());
			}
			else
			{
				TraceAssertionImpl(assertion, "Precondition: " + message.Invoke());
			}
		}

		/// <summary>
		/// Precondition check.
		/// </summary>
		public static void DoRequire(bool assertion, string message, Exception inner)
		{
			if (UseExceptions)
			{
				if (!assertion) throw new PreconditionException(message, inner);
			}
			else
			{
				TraceAssertionImpl(assertion, "Precondition: " + message);
			}
		}

		/// <summary>
		/// Precondition check.
		/// </summary>
		public static void DoAssertLambda(bool assertion, Func<string> message, Func<Exception> inner)
		{
			if (UseExceptions)
			{
				if (!assertion) throw new PreconditionException(message.Invoke(), inner.Invoke());
			}
			else
			{
				TraceAssertionImpl(assertion, "Precondition: " + message.Invoke());
			}
		}

		/// <summary>
		///		Guaranteed precondition check.
		/// </summary>
		public static void DoAssertLambda(bool assertion, Func<Exception> getExceptionToThrow)
		{
			if (UseExceptions)
			{
				if (!assertion) throw getExceptionToThrow.Invoke();
			}
			else
			{
				TraceAssertionImpl(assertion, "Assertion failed: " + getExceptionToThrow().Message);
			}
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
			if (!assertion)
			{
				if (string.IsNullOrEmpty(exceptionMessage))
				{
					throw new InvalidOperationException();
				}
				else
				{
					throw new InvalidOperationException(exceptionMessage);
				}
			}
		}

		/// <summary>
		///		Guaranteed check with <see cref="InvalidOperationException"/> thrown if assertion fails
		/// </summary>
		/// <param name="assertion">
		///		The assertion condition
		/// </param>
		/// <param name="exceptionMessage">
		///		Delegate retrieving message to return from the thrown exception if assertion fails. Pass <see langword="null"/>
		///		to use default constructor for <see cref="InvalidOperationException"/>
		/// </param>
		public static void DoCheckOperationValid(bool assertion, Func<string> getExceptionMessage)
		{
			if (!assertion)
			{
				if (getExceptionMessage == null)
				{
					throw new InvalidOperationException();
				}
				else
				{
					throw new InvalidOperationException(getExceptionMessage());
				}
			}
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
        /// Precondition check.
        /// </summary>
        public static void DoRequire(bool assertion)
		{
			if (UseExceptions)
			{
				if (!assertion) throw new PreconditionException("Precondition failed.");
			}
			else
			{
				TraceAssertionImpl(assertion, "Precondition failed.");
			}
		}

		#endregion compulsory checks

		/// <summary>
		/// Precondition check.
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
		/// Precondition check.
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
		/// Precondition check.
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
		/// Precondition check.
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
		/// Precondition check.
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
		/// Precondition check.
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
		/// Precondition check.
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
		/// Postcondition check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT"), 
		Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void Ensure(bool assertion, string message)
		{
			if (UseExceptions) 
			{
				if (!assertion)	throw new PostconditionException(message);
			}
			else
			{
				TraceAssertionImpl(assertion, "Postcondition: " + message);
			}
		}

		/// <summary>
		/// Postcondition check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT"),
		Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void EnsureLambda(bool assertion, Func<string> message)
		{
			if (UseExceptions)
			{
				if (!assertion)
					throw new PostconditionException(message.Invoke());
			}
			else
			{
				TraceAssertionImpl(assertion, "Postcondition: " + message.Invoke());
			}
		}

		[Conditional("DBC_CHECK_ALL")]
		[Conditional("DBC_CHECK_INVARIANT")]
		[Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void EnsureLambda(bool assertion, Func<Exception> getExceptionToThrow)
		{
			if (UseExceptions)
			{
				if (!assertion) throw getExceptionToThrow.Invoke();
			}
			else
			{
				TraceAssertionImpl(assertion, "Postcondition: " + getExceptionToThrow().Message);
			}
		}

		/// <summary>
		/// Postcondition check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT"), 
		Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void Ensure(bool assertion, string message, Exception inner)
		{
			if (UseExceptions) 
			{
				if (!assertion)	throw new PostconditionException(message, inner);
			}
			else
			{
				TraceAssertionImpl(assertion, "Postcondition: " + message);
			}
		}

		/// <summary>
		/// Postcondition check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT"), 
		Conditional("DBC_CHECK_POSTCONDITION")]
        [Conditional("DEBUG")]
        public static void Ensure(bool assertion)
		{
			if (UseExceptions) 
			{
				if (!assertion)	throw new PostconditionException("Postcondition failed.");
			}
			else
			{
				TraceAssertionImpl(assertion, "Postcondition failed.");
			}
		}
		
		/// <summary>
		/// Invariant check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL")]
		[Conditional("DBC_CHECK_INVARIANT")]
        [Conditional("DEBUG")]
        public static void Invariant(bool assertion, string message)
		{
			if (UseExceptions) 
			{
				if (!assertion)	throw new InvariantException(message);
			}
			else
			{
				TraceAssertionImpl(assertion, "Invariant: " + message);
			}
		}

		/// <summary>
		/// Invariant check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT")]
        [Conditional("DEBUG")]
        public static void Invariant(bool assertion, string message, Exception inner)
		{
			if (UseExceptions) 
			{
				if (!assertion)	throw new InvariantException(message, inner);
			}
			else
			{
				TraceAssertionImpl(assertion, "Invariant: " + message);
			}
		}

        [Conditional("DEBUG")]
        public static void Invariant(bool assertion, Func<Exception> getException)
		{
			if (UseExceptions) 
			{
				if (!assertion)
				{
					throw getException();
				}
			}
			else
			{
				TraceAssertionImpl(assertion, "Invariant: ");
				TraceAssertionImpl(assertion, getException());
			}
		}

		/// <summary>
		/// Invariant check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT")]
        [Conditional("DEBUG")]
        public static void Invariant(bool assertion)
		{
			if (UseExceptions) 
			{
				if (!assertion)	throw new InvariantException("Invariant failed.");
			}
			else
			{
				TraceAssertionImpl(assertion, "Invariant failed.");
			}
		}

		/// <summary>
		/// Assertion check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL")]
        [Conditional("DEBUG")]
        public static void Assert(bool assertion, string message)
		{
			if (UseExceptions) 
			{
				if (!assertion)	throw new AssertionException(message);
			}
			else
			{
				TraceAssertionImpl(assertion, "Assertion: " + message);
			}
		}

		/// <summary>
		/// Assertion check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL")]
        [Conditional("DEBUG")]
        public static void Assert(bool assertion, string message, Exception inner)
		{
			if (UseExceptions) 
			{
				if (!assertion)	throw new AssertionException(message, inner);
			}
			else
			{
				TraceAssertionImpl(assertion, "Assertion: " + message);
			}
		}

		/// <summary>
		/// Assertion check.
		/// </summary>
		[Conditional("DBC_CHECK_ALL")]
        [Conditional("DEBUG")]
        public static void Assert(bool assertion)
		{
			if (UseExceptions) 
			{
				if (!assertion)	throw new AssertionException("Assertion failed.");
			}
			else
			{
				TraceAssertionImpl(assertion, "Assertion failed.");
			}
		}

		/// <summary>
		/// Set this if you wish to use Trace Assert statements 
		/// instead of exception handling. 
		/// (The Check class uses exception handling by default.)
		/// </summary>
		public static bool UseAssertions
		{
			get
			{
				return useAssertions;
			}
			set
			{
				useAssertions = value;
			}
		}
		
		#endregion // Interface

		#region Implementation

		// No creation
		//private Check() {}

		/// <summary>
		/// Is exception handling being used?
		/// </summary>
		private static bool UseExceptions => !useAssertions;

	    // Are trace assertion statements being used? 
		// Default is to use exception handling.
		private static bool useAssertions = false;

		private static void TraceAssertionImpl(bool assertion, object message)
		{
			if (!assertion)
			{
				_log.Error(message);
			}
		}

		#endregion // Implementation

		#region Obsolete

		/// <summary>
		/// Precondition check.
		/// </summary>
		//[Obsolete("Set Check.UseAssertions = true and then call Check.Require")]
		[Conditional("DBC_CHECK_ALL"), 
		Conditional("DBC_CHECK_INVARIANT"), 
		Conditional("DBC_CHECK_POSTCONDITION"), 
		Conditional("DBC_CHECK_PRECONDITION")]
		public static void RequireTrace(bool assertion, string message)
		{
			Trace.Assert(assertion, "Precondition: " + message);
		}


		/// <summary>
		/// Precondition check.
		/// </summary>
		[Obsolete("Set Check.UseAssertions = true and then call Check.Require")]
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT"), 
		Conditional("DBC_CHECK_POSTCONDITION"), 
		Conditional("DBC_CHECK_PRECONDITION")]
		public static void RequireTrace(bool assertion)
		{
			Trace.Assert(assertion, "Precondition failed.");
		}
		
		/// <summary>
		/// Postcondition check.
		/// </summary>
		[Obsolete("Set Check.UseAssertions = true and then call Check.Ensure")]
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT"), 
		Conditional("DBC_CHECK_POSTCONDITION")] 
		public static void EnsureTrace(bool assertion, string message)
		{
			Trace.Assert(assertion, "Postcondition: " + message);
		}

		/// <summary>
		/// Postcondition check.
		/// </summary>
		[Obsolete("Set Check.UseAssertions = true and then call Check.Ensure")]
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT"), 
		Conditional("DBC_CHECK_POSTCONDITION")] 
		public static void EnsureTrace(bool assertion)
		{
			Trace.Assert(assertion, "Postcondition failed.");
		}
		
		/// <summary>
		/// Invariant check.
		/// </summary>
		[Obsolete("Set Check.UseAssertions = true and then call Check.Invariant")]
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT")] 
		public static void InvariantTrace(bool assertion, string message)
		{
			Trace.Assert(assertion, "Invariant: " + message);
		}

		/// <summary>
		/// Invariant check.
		/// </summary>
		[Obsolete("Set Check.UseAssertions = true and then call Check.Invariant")]
		[Conditional("DBC_CHECK_ALL"),
		Conditional("DBC_CHECK_INVARIANT")] 
		public static void InvariantTrace(bool assertion)
		{
			Trace.Assert(assertion, "Invariant failed.");
		}

		/// <summary>
		/// Assertion check.
		/// </summary>
		[Obsolete("Set Check.UseAssertions = true and then call Check.Assert")]
		[Conditional("DBC_CHECK_ALL")]
		public static void AssertTrace(bool assertion, string message)
		{
			Trace.Assert(assertion, "Assertion: " + message);
		}

		/// <summary>
		/// Assertion check.
		/// </summary>
		[Obsolete("Set Check.UseAssertions = true and then call Check.Assert")]
		[Conditional("DBC_CHECK_ALL")]
		public static void AssertTrace(bool assertion)
		{
			Trace.Assert(assertion, "Assertion failed.");
		}
		#endregion // Obsolete

	} // End Check

	#region Exceptions

	/// <summary>
	/// Exception raised when a contract is broken.
	/// Catch this exception type if you wish to differentiate between 
	/// any DesignByContract exception and other runtime exceptions.
	///  
	/// </summary>
	public class DesignByContractException : ApplicationException
	{
		protected DesignByContractException() {}
		protected DesignByContractException(string message) : base(message) {}
		protected DesignByContractException(string message, Exception inner) : base(message, inner) {}
	}

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

	/// <summary>
	/// Exception raised when an invariant fails.
	/// </summary>
	[Serializable]
	public class InvariantException : DesignByContractException
	{
		/// <summary>
		/// Invariant Exception.
		/// </summary>
		public InvariantException() {}
		/// <summary>
		/// Invariant Exception.
		/// </summary>
		public InvariantException(string message) : base(message) {}
		/// <summary>
		/// Invariant Exception.
		/// </summary>
		public InvariantException(string message, Exception inner) : base(message, inner) {}
	}

	/// <summary>
	/// Exception raised when an assertion fails.
	/// </summary>
	[Serializable]
	public class AssertionException : DesignByContractException
	{
		/// <summary>
		/// Assertion Exception.
		/// </summary>
		public AssertionException() {}
		/// <summary>
		/// Assertion Exception.
		/// </summary>
		public AssertionException(string message) : base(message) {}
		/// <summary>
		/// Assertion Exception.
		/// </summary>
		public AssertionException(string message, Exception inner) : base(message, inner) {}
	}

	#endregion // Exception classes

} // End Design By Contract
