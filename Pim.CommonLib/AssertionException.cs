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

namespace Pim.CommonLib;
// End Check

#region Exceptions

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

// End Design By Contract
