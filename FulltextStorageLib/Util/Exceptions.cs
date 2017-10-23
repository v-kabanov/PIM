// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-09-26
// Comment		
// **********************************************************************************************/

using System;
using System.Text;
using Pim.CommonLib;

namespace FulltextStorageLib.Util
{
    /// <summary>
    ///     Exception helpers.
    /// </summary>
    public static class Exceptions
    {
        /// <summary>
        ///     Look up first exception of a particular type in exception stack starting right from <paramref name="exception"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of exception to return
        /// </typeparam>
        /// <param name="exception">
        ///     Mandatory
        /// </param>
        /// <returns>
        ///     Null if exception stack (defined by <see cref="Exception.InnerException"/>) does not contain <typeparamref name="T"/>.
        /// </returns>
        public static T LookUpInHierarchy<T>(Exception exception)
            where T: Exception
        {
            Check.DoRequireArgumentNotNull(exception, nameof(exception));

            T result = null;

            do
            {
                result = exception as T;
            }
            while (result == null && (exception = exception.InnerException) != null);

            return result;
        }

        /// <summary>
        ///     Get only messages from given and all nested exceptions.
        /// </summary>
        /// <param name="exception">
        ///     Mandatory
        /// </param>
        /// <returns>
        ///     String with messages starting from top, one per line.
        /// </returns>
        public static string GetHierarchyMessage(Exception exception)
        {
            Check.DoRequireArgumentNotNull(exception, nameof(exception));

            var result = new StringBuilder();
            var level = 0;
            string lastMessage = null;
            for (var currentException = exception; currentException != null; currentException = currentException.InnerException)
            {
                if (currentException.Message != lastMessage)
                {
                    result.Append(' ', level).Append("-").Append(currentException.Message).AppendLine();
                    lastMessage = currentException.Message;
                    ++level;
                }
            }

            return result.ToString();
        }

        /// <summary>
        ///     Get innermost nested exception
        /// </summary>
        /// <param name="exception">
        ///     null allowed
        /// </param>
        /// <returns>
        ///     null if <paramref name="exception"/> is null
        /// </returns>
        public static Exception GetInnermost(Exception exception)
        {
            if (exception == null) return null;

            var result = exception;

            while (result.InnerException != null)
                result = result.InnerException;

            return result;
        }
    }
}