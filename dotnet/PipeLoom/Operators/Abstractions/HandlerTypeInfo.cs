using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Operators.Abstractions;

public sealed class HandlerTypeInfo
{
    public const int ResultTypePosition = -1;

    public static HandlerTypeInfo OfReturnType(OperatorHandler handler)
    {
        return new HandlerTypeInfo(handler, ResultTypePosition);
    }

    public static HandlerTypeInfo OfArgumentType(OperatorHandler handler, byte position)
    {
        return new HandlerTypeInfo(handler, position);
    }
    
    public OperatorHandler Handler { get; }
    public int TypePosition { get; }

    public PlTypeDef SignatureType => this.GetSignatureType();

    public PlTypeDef InferredType => this.GetConstrained();
   
    private HandlerTypeInfo(OperatorHandler handler, int typePosition)
    {
        this.Handler = handler;
        this.TypePosition = typePosition;
    }
    
    

    private PlTypeDef GetConstrained()
    {
        return this.SignatureType;
    }
    
    private PlTypeDef GetSignatureType()
    {
        return this.TypePosition == ResultTypePosition
            ? this.Handler.Signature.ReturnType
            : this.Handler.Signature.ArgumentTypes[this.TypePosition];
    }
    
    
}