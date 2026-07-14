using System;

namespace PipeLoom.Operators.Abstractions;

public enum PlOperatorArity
{
    Nullary = 0,
    Unary = 1,
    Binary = 2,
    Ternary = 3,
    Variadic = 7
}

public static class PlOperatorArityExtensions {
    public static string ToDisplayString(this PlOperatorArity arity)
    {
        return arity switch
        {
            PlOperatorArity.Nullary => "Nullary",
            PlOperatorArity.Unary => "Unary",
            PlOperatorArity.Binary => "Binary",
            PlOperatorArity.Ternary => "Ternary",
            PlOperatorArity.Variadic => "Variadic",
            _ => throw new ArgumentOutOfRangeException(nameof(arity), arity, "Unknown arity")

        };
    }
}