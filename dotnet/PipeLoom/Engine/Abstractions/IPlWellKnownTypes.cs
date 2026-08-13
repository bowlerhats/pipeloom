namespace PipeLoom.Engine.Abstractions;

public interface IPlWellknown
{
    PlVariant Variant { get; }
    PlVoid Void { get; }
    
    PlBundle Bundle { get; }
    // PlMany Many { get; }
    
    PlMany ManyOfVariant { get; }
    
    //PlNever Never { get; }
}