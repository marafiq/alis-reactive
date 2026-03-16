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
        internal static bool HasValidatorTypes(List<Entry> entries)
        {
            foreach (var entry in entries)
                if (ReactionHasValidatorType(entry.Reaction))
                    return true;
            return false;
        }

        private static bool ReactionHasValidatorType(Reaction reaction)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    return RequestHasValidatorType(hr.Request);
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        if (RequestHasValidatorType(req))
                            return true;
                    return false;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        if (ReactionHasValidatorType(branch.Reaction))
                            return true;
                    return false;
                default:
                    return false;
            }
        }

        private static bool RequestHasValidatorType(RequestDescriptor req)
        {
            if (req.ValidatorType != null)
                return true;
            return req.Chained != null && RequestHasValidatorType(req.Chained);
        }

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
                    req.EnrichValidation(extracted);
            }

            if (req.Chained != null)
                ResolveRequest(req.Chained, extractor);
        }
    }
}
