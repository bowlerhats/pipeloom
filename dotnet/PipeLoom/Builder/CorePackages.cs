using PipeLoom.Types.Scalars;
using PipeLoom.Types.Scalars.Numerical;

namespace PipeLoom.Builder;

public static class CorePackages
{
    extension<TBuilder>(TBuilder builder) where TBuilder: PipeLoomBuilder<TBuilder>
    {
        public TBuilder AddCoreNumbers()
        {
            const string regToken = "core.numbers";
            if (builder.IsRegistered(regToken))
                return builder;
        
            builder.AddType(engine => new PlByte(engine));
            builder.AddType(engine => new PlInteger(engine));
            builder.AddType(engine => new PlLong(engine));
            builder.AddType(engine => new PlDouble(engine));
        
            builder.AddConverters(CoreNumberConverters.AddStandardNumberConverters);
            builder.AddConverters(CoreNumberConverters.AddTensorConverters);
        
            builder.Registered(regToken);
        
            return builder;
        }

        public TBuilder AddCoreMath()
        {
            builder.AddCoreNumbers();
            
            const string regToken = "core.math";
            if (builder.IsRegistered(regToken))
                return builder;
        
            builder.AddOperatorClass(d => new PlSum(d));
        
            builder.Registered(regToken);
        
            return builder;
        }

        public TBuilder AddExtendedNumbers()
        {
            builder.AddCoreNumbers();
            
            const string regToken = "extended.numbers";
            if (builder.IsRegistered(regToken))
                return builder;
            
            builder.AddType(engine => new PlShort(engine));
            
            builder.AddType(engine => new PlUshort(engine));
            builder.AddType(engine => new PlUint(engine));
            builder.AddType(engine => new PlUlong(engine));
            
            builder.AddType(engine => new PlDecimal(engine));
            
            builder.AddConverters(CoreNumberConverters.AddExtendedNumberConverters);
            builder.AddConverters(CoreNumberConverters.AddExtendedTensorConverters);

            builder.Registered(regToken);
            
            return builder;
        }
        
        public TBuilder AddExtendedMath()
        {
            builder.AddCoreMath();
            builder.AddExtendedNumbers();
            
            const string regToken = "extended.math";
            if (builder.IsRegistered(regToken))
                return builder;

            builder.AddOperatorClass(engine => new PlSumExtended(engine));
            
            builder.Registered(regToken);

            return builder;
        }
    }
}