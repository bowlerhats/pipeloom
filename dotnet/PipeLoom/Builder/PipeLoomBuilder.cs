using System;
using System.Collections.Generic;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions.Registration;

namespace PipeLoom.Builder;

public sealed class PipeLoomBuilder : PipeLoomBuilder<PipeLoomBuilder>
{
    public static PipeLoomBuilder Create()
    {
        return new PipeLoomBuilder()
            .AddCoreTypes();
    }
    
    private PipeLoomBuilder() { }
    
    public override IPipeLoomEngine Build()
    {
        return new PipeLoomEngine(this);
    }
}

public abstract class PipeLoomBuilder<TSelf> : IEngineConfig
{
    public List<Func<IPipeLoomEngine, PlTypeDef>> TypeFactories { get; set; } = [];
    public List<Func<IPipeLoomEngine, IPlOperatorClass>> OperatorClassFactories { get; set; } = [];
    
    public List<(string name, Func<PlOperatorRegistrator, PlOperatorRegistrator> regFunc)> OperatorFactories { get; set; } = [];
    
    protected TSelf Self => (TSelf)(object)this;

    public abstract IPipeLoomEngine Build();

    public virtual TSelf AddCoreTypes()
    {
        this.AddType(engine => new PlNever(engine));
        
        return this.Self;
    }

    public virtual TSelf AddCoreOperators()
    {
        this.AddOperatorClass(d => new PlOpIsNull(d));
        this.AddOperatorClass(d => new PlOpIsNotNull(d));
        this.AddOperatorClass(d => new PlOpPipe(d));
        
        return this.Self;
    }

    public virtual TSelf AddType(Func<IPipeLoomEngine, PlTypeDef> factory)
    {
        this.TypeFactories.Add(factory);
        
        return this.Self;
    }

    public virtual TSelf AddOperatorClass(Func<IPipeLoomEngine, IPlOperatorClass> factory)
    {
        this.OperatorClassFactories.Add(factory);
        return this.Self;
    }

    public virtual TSelf AddOperator(string name, Func<PlOperatorRegistrator, PlOperatorRegistrator> regFunc)
    {
        this.OperatorFactories.Add((name, regFunc));
        return this.Self;
    }
    
}
