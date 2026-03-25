using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Resolvers
{
    /// <summary>
    /// Walks the reaction tree and resolves validation rules from an IValidationExtractor.
    /// After extraction, enriches validation fields from the ComponentsMap.
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

        internal static void Resolve(
            List<Entry> entries,
            IValidationExtractor extractor,
            IReadOnlyDictionary<string, ComponentRegistration>? componentsMap = null)
        {
            foreach (var entry in entries)
                ResolveReaction(entry.Reaction, extractor, componentsMap);
        }

        private static void ResolveReaction(
            Reaction reaction,
            IValidationExtractor extractor,
            IReadOnlyDictionary<string, ComponentRegistration>? componentsMap)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveRequest(hr.Request, extractor, componentsMap);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveRequest(req, extractor, componentsMap);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveReaction(branch.Reaction, extractor, componentsMap);
                    break;
            }
        }

        private static void ResolveRequest(
            RequestDescriptor req,
            IValidationExtractor extractor,
            IReadOnlyDictionary<string, ComponentRegistration>? componentsMap)
        {
            if (req.ValidatorType != null && req.Validation != null)
            {
                var formId = req.Validation.FormId;
                var extracted = extractor.ExtractRules(req.ValidatorType, formId);
                if (extracted == null)
                {
                    throw new System.InvalidOperationException(
                        $"Validator '{req.ValidatorType.Name}' produced no client rules for form '{formId}'. " +
                        "Ensure the validator is registered in the factory and has extractable rules.");
                }
                req.EnrichValidation(extracted);
            }

            if (req.Validation != null && componentsMap != null)
                EnrichFieldsFromComponents(req.Validation, componentsMap);

            if (req.Chained != null)
                ResolveRequest(req.Chained, extractor, componentsMap);
        }

        internal static void EnrichFromComponents(
            List<Entry> entries,
            IReadOnlyDictionary<string, ComponentRegistration> componentsMap)
        {
            foreach (var entry in entries)
                EnrichReaction(entry.Reaction, componentsMap);
        }

        private static void EnrichReaction(
            Reaction reaction,
            IReadOnlyDictionary<string, ComponentRegistration> componentsMap)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    EnrichRequest(hr.Request, componentsMap);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        EnrichRequest(req, componentsMap);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        EnrichReaction(branch.Reaction, componentsMap);
                    break;
            }
        }

        private static void EnrichRequest(
            RequestDescriptor req,
            IReadOnlyDictionary<string, ComponentRegistration> componentsMap)
        {
            if (req.Validation != null)
                EnrichFieldsFromComponents(req.Validation, componentsMap);
            if (req.Chained != null)
                EnrichRequest(req.Chained, componentsMap);
        }

        private static void EnrichFieldsFromComponents(
            ValidationDescriptor desc,
            IReadOnlyDictionary<string, ComponentRegistration> componentsMap)
        {
            foreach (var field in desc.Fields)
            {
                if (componentsMap.TryGetValue(field.FieldName, out var registration))
                {
                    field.FieldId = registration.ComponentId;
                    field.Vendor = registration.Vendor;
                    field.ReadExpr = registration.ReadExpr;
                    field.CoerceAs = registration.CoerceAs;
                }
            }
        }
        internal static void StampPlanId(List<Entry> entries, string planId)
        {
            foreach (var entry in entries)
                StampReaction(entry.Reaction, planId);
        }

        private static void StampReaction(Reaction reaction, string planId)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    StampRequest(hr.Request, planId);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        StampRequest(req, planId);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        StampReaction(branch.Reaction, planId);
                    break;
            }
        }

        private static void StampRequest(RequestDescriptor req, string planId)
        {
            if (req.Validation != null)
                req.Validation.PlanId = planId;
            if (req.Chained != null)
                StampRequest(req.Chained, planId);
        }
    }
}
