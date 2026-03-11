using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Resolvers
{
    /// <summary>
    /// Walks the reaction tree and resolves validation rules from an IValidationExtractor.
    /// Builder-phase metadata (ValidatorType, ReadExprOverrides) is read from the
    /// RequestBuildContext dictionary — never from RequestDescriptor itself.
    /// </summary>
    internal static class ValidationResolver
    {
        internal static void Resolve(
            List<Entry> entries,
            IValidationExtractor extractor,
            IReadOnlyDictionary<RequestDescriptor, RequestBuildContext> buildContexts)
        {
            foreach (var entry in entries)
            {
                ResolveReaction(entry.Reaction, extractor, buildContexts);
            }
        }

        private static void ResolveReaction(
            Reaction reaction,
            IValidationExtractor extractor,
            IReadOnlyDictionary<RequestDescriptor, RequestBuildContext> buildContexts)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveRequest(hr.Request, extractor, buildContexts);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveRequest(req, extractor, buildContexts);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveReaction(branch.Reaction, extractor, buildContexts);
                    break;
            }
        }

        private static void ResolveRequest(
            RequestDescriptor req,
            IValidationExtractor extractor,
            IReadOnlyDictionary<RequestDescriptor, RequestBuildContext> buildContexts)
        {
            buildContexts.TryGetValue(req, out var ctx);

            if (ctx?.ValidatorType != null && req.Validation != null)
            {
                var formId = req.Validation.FormId;
                var extracted = extractor.ExtractRules(ctx.ValidatorType, formId);
                if (extracted != null)
                {
                    req.Validation = extracted;
                }
            }

            if (ctx?.ReadExprOverrides != null && req.Validation != null)
            {
                foreach (var kvp in ctx.ReadExprOverrides)
                {
                    req.Validation = req.Validation.WithReadExpr(kvp.Key, kvp.Value);
                }
            }

            if (req.Chained != null)
            {
                ResolveRequest(req.Chained, extractor, buildContexts);
            }
        }
    }
}
