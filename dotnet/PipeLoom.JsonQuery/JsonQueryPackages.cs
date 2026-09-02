using System;
using System.Text.Json.Nodes;
using PipeLoom.Builder;
using PipeLoom.Engine;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.JsonQuery.Operators;
using PipeLoom.JsonQuery.Operators.FactoryOps;
using PipeLoom.JsonQuery.Parsing;
using PipeLoom.JsonQuery.Types;
using PipeLoom.Operators.CoreControlFlow;
using PipeLoom.Types.Scalars.Numerical;

namespace PipeLoom.JsonQuery;

public static class JsonQueryExtensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : PipeLoomBuilder<TBuilder>
    {
        public TBuilder AddJsonQuery()
        {
            const string regToken = "jsonquery";
            if (builder.IsRegistered(regToken))
                return builder;
            
            // todo: support optional additions: don't add if exists...
            builder.AddType(engine => new PlDecimal(engine));
            builder.AddOperatorClass(engine => new PlOpPipe(engine));

            builder.AddType(engine => new PlJsonNode(engine));

            // builder.AddOperatorClass(engine => new JsOpPipe(engine));
            builder.AddOperatorClass(engine => new JsOpGet(engine));
            builder.AddOperatorClass(engine => new JsOpObject(engine));
            builder.AddOperatorClass(engine => new JsOpArray(engine));
            

            builder.Registered(regToken);
            return builder;
        }
    }

    extension<TPlan>(TPlan plan) where TPlan : WeavePlan
    {
        public TPlan AppendJsonQuery(string jsq)
        {
            plan.RootNode.AppendJsonQuery(jsq);
            return plan;
        }

        public TPlan AppendJsonQuery(JsonNode jsqNode)
        {
            plan.RootNode.AppendJsonQuery(jsqNode);
            return plan;
        }
    }

    extension(WeaveNode node)
    {
        public WeaveNode AppendJsonQuery(string jsq)
        {
            if (string.IsNullOrWhiteSpace(jsq))
                return node;
            
            var jsqNode = JsonQueryParser.Parse(jsq);
            if (jsqNode is null)
                return node;

            return node.AppendJsonQuery(jsqNode);
        }

        public WeaveNode AppendJsonQuery(JsonNode? jsqNode)
        {
            if (jsqNode is not JsonArray jsArray)
                throw new PipeLoomException("Invalid jsqNode to plan, Expected JsonArray");
            
            if (jsArray.Count == 0)
                throw new PipeLoomException("Empty json query");

            PlanBuilder.Build(node, jsArray);

            return node;
        }
    }
}