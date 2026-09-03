using PipeLoom.Builder;
using PipeLoom.JsonQuery.Operators.Control;
using PipeLoom.JsonQuery.Operators.Factories;
using PipeLoom.JsonQuery.Operators.Logical;
using PipeLoom.JsonQuery.Operators.Mappers;
using PipeLoom.JsonQuery.Operators.Maths;
using PipeLoom.JsonQuery.Operators.Projectors;
using PipeLoom.JsonQuery.Operators.RegexOps;
using PipeLoom.JsonQuery.Operators.Relational;

namespace PipeLoom.JsonQuery.Operators;

internal static class OperatorRegs
{
    public static void AddOperators<TBuilder>(TBuilder builder)
        where TBuilder: PipeLoomBuilder<TBuilder>
    {
        // Control
        builder.AddOperatorClass(engine => new JsOpIf(engine));
        
        // Factories
        builder.AddOperatorClass(engine => new JsOpObject(engine));
        builder.AddOperatorClass(engine => new JsOpArray(engine));
        builder.AddOperatorClass(engine => new JsOpNumber(engine));
        builder.AddOperatorClass(engine => new JsOpString(engine));
        
        // Logical
        builder.AddOperatorClass(engine => new JsOpAnd(engine));
        builder.AddOperatorClass(engine => new JsOpEq(engine));
        
        // Mappers
        builder.AddOperatorClass(engine => new JsOpMap(engine));
        builder.AddOperatorClass(engine => new JsOpMapObject(engine));
        
        // Math
        builder.AddOperatorClass(engine => new JsOpSum(engine));
        builder.AddOperatorClass(engine => new JsOpAdd(engine));
        builder.AddOperatorClass(engine => new JsOpSubtract(engine));
        builder.AddOperatorClass(engine => new JsOpMultiply(engine));
        builder.AddOperatorClass(engine => new JsOpDivide(engine));
        builder.AddOperatorClass(engine => new JsOpPow(engine));
        builder.AddOperatorClass(engine => new JsOpMod(engine));
        builder.AddOperatorClass(engine => new JsOpAbs(engine));
        builder.AddOperatorClass(engine => new JsOpRound(engine));
        
        // Projectors
        builder.AddOperatorClass(engine => new JsOpGet(engine));
        builder.AddOperatorClass(engine => new JsOpPick(engine));
        
        // Regex
        builder.AddOperatorClass(engine => new JsOpRegex(engine));
        
        // Relational
        builder.AddOperatorClass(engine => new JsOpFilter(engine));
        builder.AddOperatorClass(engine => new JsOpSort(engine));
        builder.AddOperatorClass(engine => new JsOpReverse(engine));
        
    }
    
}