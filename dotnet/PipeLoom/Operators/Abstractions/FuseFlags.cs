using System;

namespace PipeLoom.Operators.Abstractions;

[Flags]
public enum PreFuseFlags
{
    None = 0,
    SkipChildFuse = 1
}

[Flags]
public enum PostFuseFlags
{
    None = 0
}

[Flags]
public enum NodeFuseFlags
{
    None = 0,
    // DiscardSelf = 2
}
