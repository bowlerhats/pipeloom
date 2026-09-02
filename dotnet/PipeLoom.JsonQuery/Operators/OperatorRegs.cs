using PipeLoom.Builder;
using PipeLoom.JsonQuery.Operators.Control;
using PipeLoom.JsonQuery.Operators.Factories;
using PipeLoom.JsonQuery.Operators.Logical;
using PipeLoom.JsonQuery.Operators.Mappers;
using PipeLoom.JsonQuery.Operators.Math;
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
        
        // Logical
        builder.AddOperatorClass(engine => new JsOpAnd(engine));
        builder.AddOperatorClass(engine => new JsOpEq(engine));
        
        // Mappers
        builder.AddOperatorClass(engine => new JsOpMap(engine));
        
        // Math
        builder.AddOperatorClass(engine => new JsOpSum(engine));
        
        // Projectors
        builder.AddOperatorClass(engine => new JsOpGet(engine));
        
        // Regex
        builder.AddOperatorClass(engine => new JsOpRegex(engine));
        
        // Relational
        builder.AddOperatorClass(engine => new JsOpFilter(engine));
    }
    
}