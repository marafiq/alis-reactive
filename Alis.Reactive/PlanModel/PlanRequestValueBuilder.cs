using System;
using System.Collections.Generic;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.PlanModel
{
    internal static class PlanRequestValueBuilder
    {
        internal static ValueExpr Build(
            IReadOnlyList<RequestValuePart> requestValues,
            AuthoredPlan plan,
            PlanValueCompiler values)
        {
            if (requestValues.Count == 1 && requestValues[0] is IncludeAllBindingsRequestValue)
                return new BindingMapValueExpr("all");

            var fields = new Dictionary<string, ValueExpr>(StringComparer.Ordinal);
            foreach (var requestValue in requestValues)
            {
                if (requestValue is IncludeAllBindingsRequestValue)
                {
                    foreach (var binding in plan.Bindings.Keys)
                        AddNestedField(fields, binding, new BindingValueExpr(binding));
                    continue;
                }

                if (requestValue is LiteralRequestValue literal)
                {
                    AddNestedField(fields, literal.Key, new LiteralValueExpr(literal.Value));
                    continue;
                }

                if (requestValue is ContextRequestValue context)
                {
                    AddNestedField(fields, context.Key, context.Value);
                    continue;
                }

                if (requestValue is ComponentRequestValue component)
                {
                    AddNestedField(
                        fields,
                        component.Key,
                        values.CreateComponentValue(component.ComponentId, component.Component, component.Binding, component.Shape));
                }
            }

            return new ObjectValueExpr(fields);
        }

        private static void AddNestedField(Dictionary<string, ValueExpr> fields, string path, ValueExpr value)
        {
            var segments = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                return;

            var current = fields;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (!current.TryGetValue(segments[i], out var existing) || !(existing is ObjectValueExpr objectExpr))
                {
                    objectExpr = new ObjectValueExpr(new Dictionary<string, ValueExpr>(StringComparer.Ordinal));
                    current[segments[i]] = objectExpr;
                }

                current = objectExpr.Fields;
            }

            current[segments[segments.Length - 1]] = value;
        }
    }
}
