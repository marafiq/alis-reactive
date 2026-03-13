using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Resolvers
{
    /// <summary>
    /// Walks the reaction tree and resolves validation rules from an IValidationExtractor.
    /// Uses req.ValidatorType directly — no build contexts, no components map.
    /// </summary>
    internal static class ValidationResolver
    {
        internal static void Resolve(List<Entry> entries, IValidationExtractor extractor)
        {
            foreach (var entry in entries)
                ResolveReaction(entry.Reaction, extractor);
        }

        private static void ResolveReaction(Reaction reaction, IValidationExtractor extractor)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveRequest(hr.Request, extractor);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveRequest(req, extractor);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveReaction(branch.Reaction, extractor);
                    break;
            }
        }

        private static void ResolveRequest(RequestDescriptor req, IValidationExtractor extractor)
        {
            if (req.ValidatorType != null && req.Validation != null)
            {
                var formId = req.Validation.FormId;
                var extracted = extractor.ExtractRules(req.ValidatorType, formId);
                if (extracted != null)
                    req.Validation = extracted;
            }

            if (req.Chained != null)
                ResolveRequest(req.Chained, extractor);
        }
    }
}
