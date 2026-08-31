using System;
using System.Collections.Generic;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Registration;
using PipeLoom.Types.Scalars;
using PipeLoom.Types.Scalars.Numerical;

namespace PipeLoom.Builder;

public sealed class PipeLoomBuilder : PipeLoomBuilder<PipeLoomBuilder>
{
    public static PipeLoomBuilder Create()
    {
        return new PipeLoomBuilder()
            .AddCoreTypes()
            .AddCoreOperators()
            .AddCoreConverters();
    }
    
    private PipeLoomBuilder() { }
    
    public override PipeLoomEngine Build()
    {
        return new PipeLoomEngine(this);
    }
}

public abstract class PipeLoomBuilder<TSelf> : IEngineConfig
{
    public List<Func<IPipeLoomEngine, PlTypeDef>> TypeFactories { get; set; } = [];
    public List<Func<IPipeLoomEngine, PlOperatorClass>> OperatorClassFactories { get; set; } = [];
    
    public List<(string name, Func<PlOperatorRegistrator, PlOperatorRegistrator> regFunc)> OperatorFactories { get; set; } = [];

    public List<Action<ConverterRegistrator>> GlobalConverterRegistrations { get; set; } = [];

    protected TSelf Self => (TSelf)(object)this;

    public HashSet<string> RegisteredTokens { get; } = [];

    public abstract PipeLoomEngine Build();

    public bool IsRegistered(string token)
    {
        return this.RegisteredTokens.Contains(token);
    }
    
    public TSelf Registered(string token)
    {
        this.RegisteredTokens.Add(token);
        return this.Self;
    }

    protected virtual TSelf AddCoreTypes()
    {
        const string regToken = "core.types";
        if (this.IsRegistered(regToken))
            return this.Self;
        
        this.AddType(engine => new PlVariant(engine));
        this.AddType(engine => new PlVoid(engine));
        
        this.AddType(engine => new PlGenericDetached(engine));
        this.AddType(engine => new PlGenericScalar(engine));
        this.AddType(engine => new PlGenericMany(engine));
        this.AddType(engine => new PlGenericBundle(engine));
        this.AddType(engine => new PlGenericReadOnlyBundle(engine));

        this.AddType(engine => new PlText(engine));
        this.AddType(engine => new PlBool(engine));
        
        this.Registered(regToken);
        
        return this.Self;
    }

    protected virtual TSelf AddCoreOperators()
    {
        const string regToken = "core.operators";
        if (this.IsRegistered(regToken))
            return this.Self;
        
        this.AddOperatorClass(d => new PlOpConstant(d));
        this.AddOperatorClass(d => new PlOpLog(d));
        
        this.AddOperatorClass(d => new PlOpSequence(d));
        this.AddOperatorClass(d => new PlOpPipe(d));
        
        this.AddOperatorClass(d => new PlOpIsNull(d));
        this.AddOperatorClass(d => new PlOpIsNotNull(d));
        
        this.Registered(regToken);
        
        return this.Self;
    }

    protected virtual TSelf AddCoreConverters()
    {
        const string regToken = "core.converters";
        if (this.IsRegistered(regToken))
            return this.Self;
        
        

        this.Registered(regToken);
        
        return this.Self;
    }

    public virtual TSelf AddType(Func<IPipeLoomEngine, PlTypeDef> factory)
    {
        this.TypeFactories.Add(factory);
        
        return this.Self;
    }

    public virtual TSelf AddOperatorClass(Func<IPipeLoomEngine, PlOperatorClass> factory)
    {
        this.OperatorClassFactories.Add(factory);
        
        return this.Self;
    }

    public virtual TSelf AddOperator(string name, Func<PlOperatorRegistrator, PlOperatorRegistrator> regFunc)
    {
        this.OperatorFactories.Add((name, regFunc));
        
        return this.Self;
    }

    public virtual TSelf AddConverters(Action<ConverterRegistrator> convertible)
    {
        this.GlobalConverterRegistrations.Add(convertible);

        return this.Self;
    }
}
