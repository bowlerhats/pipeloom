using System;

namespace PipeLoom.Engine.Abstractions.Errors;

public class PipeLoomException : Exception
{
    public PipeLoomException()
    {
    }

    public PipeLoomException(string message) : base(message)
    {
    }

    public PipeLoomException(string message, Exception inner) : base(message, inner)
    {
    }
}