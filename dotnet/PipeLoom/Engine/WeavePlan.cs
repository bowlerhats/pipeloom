using System;

namespace PipeLoom.Engine;

public class WeavePlan
{
    public IPipeLoomEngine Engine => throw new NotImplementedException();
    public WeaveNode RootNode => throw new NotImplementedException();
}

public class WeaveNode;