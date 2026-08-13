using System;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Engine;

internal sealed class PlWellknown : IPlWellknown
{
    public required PipeLoomEngine Engine { get; init; }

    public PlVariant Variant => field ??= this.GetType<PlVariant>();
    public PlVoid Void => field ??= this.GetType<PlVoid>();

    public PlBundle Bundle => field ??= this.GetType<PlBundle>();
    public PlMany Many => field ??= this.GetType<PlMany>();

    public PlMany ManyOfVariant => throw new NotImplementedException(); 
    
    private PlTypeDef TypeOf<T>()
    {
        return this.Engine.TypeOf<T>();
    }

    private T GetType<T>()
        where T: PlTypeDef
    {
        return this.Engine.GetType<T>();
    }
}