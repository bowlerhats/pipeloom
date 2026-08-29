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

    public abstract PipeLoomEngine Build();

    public virtual TSelf AddCoreTypes()
    {
        this.AddType(engine => new PlVariant(engine));
        this.AddType(engine => new PlVoid(engine));
        
        this.AddType(engine => new PlGenericDetached(engine));
        this.AddType(engine => new PlGenericScalar(engine));
        this.AddType(engine => new PlGenericMany(engine));
        this.AddType(engine => new PlGenericBundle(engine));
        this.AddType(engine => new PlGenericReadOnlyBundle(engine));

        this.AddType(engine => new PlText(engine));
        this.AddType(engine => new PlBool(engine));
        
        this.AddType(engine => new PlByte(engine));
        this.AddType(engine => new PlShort(engine));
        this.AddType(engine => new PlInteger(engine));
        this.AddType(engine => new PlLong(engine));
        
        this.AddType(engine => new PlUshort(engine));
        this.AddType(engine => new PlUint(engine));
        this.AddType(engine => new PlUlong(engine));
        
        this.AddType(engine => new PlDouble(engine));
        this.AddType(engine => new PlDecimal(engine));
        
        return this.Self;
    }

    public virtual TSelf AddCoreOperators()
    {
        this.AddOperatorClass(d => new PlOpConstant(d));
        this.AddOperatorClass(d => new PlOpLog(d));
        
        this.AddOperatorClass(d => new PlOpSequence(d));
        this.AddOperatorClass(d => new PlOpPipe(d));
        
        this.AddOperatorClass(d => new PlOpIsNull(d));
        this.AddOperatorClass(d => new PlOpIsNotNull(d));
        
        this.AddOperatorClass(d => new PlSum(d));
        
        return this.Self;
    }

    public virtual TSelf AddCoreConverters()
    {
        this.AddConverters(CoreNumberConverters.AddStandardNumberConverters);
        this.AddConverters(CoreNumberConverters.AddTensorConverters);
        
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
