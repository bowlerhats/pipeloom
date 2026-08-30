namespace PipeLoom.Engine.Abstractions;

public interface IPlWellknown
{
    PlVariant Variant { get; }
    PlVoid Void { get; }
    
    // PlBundle Bundle { get; }
    
    // PlReadOnlyBundleOf ReadOnlyBundleOfVariant { get; }
    // PlMany Many { get; }
    
    // PlManyOf ManyOfVariant { get; }
    
    //PlNever Never { get; }
}