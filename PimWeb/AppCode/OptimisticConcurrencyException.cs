using System;
using System.Runtime.Serialization;

namespace PimWeb.AppCode;

public class OptimisticConcurrencyException : Exception
{
    /// <inheritdoc />
    public OptimisticConcurrencyException()
    {
    }

    /// <inheritdoc />
    protected OptimisticConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc />
    public OptimisticConcurrencyException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public OptimisticConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}