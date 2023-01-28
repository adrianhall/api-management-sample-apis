// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Todo.Data;

/// <summary>
/// The base exception class for all repository exceptions.
/// </summary>
public class RepositoryException : Exception
{
    /// <summary>
    /// Creates a new <see cref="RepositoryException"/>
    /// </summary>
    public RepositoryException()
    {
    }
    
    /// <summary>
    /// Creates a new <see cref="RepositoryException"/> with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public RepositoryException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new <see cref="RepositoryException"/> with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public RepositoryException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected RepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
