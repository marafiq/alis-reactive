using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Resolvers;

namespace Alis.Reactive
{
    public class ReactivePlan<TModel> : IReactivePlan<TModel> where TModel : class
    {
        private static readonly JsonSerializerOptions CompactOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions FormattedOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly Dictionary<string, ComponentRegistration> _componentsMap = new Dictionary<string, ComponentRegistration>();

        public ReactivePlan(bool isPartial = false) { IsPartial = isPartial; }

        public string PlanId { get; } = typeof(TModel).FullName!;
        public bool IsPartial { get; }

        public IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap => _componentsMap;

        public void AddEntry(Entry entry)
        {
            _entries.Add(entry);
        }

        public void AddToComponentsMap(string bindingPath, ComponentRegistration entry)
        {
            if (_componentsMap.TryGetValue(bindingPath, out var existing))
            {
                if (existing.ComponentId == entry.ComponentId
                    && existing.Vendor == entry.Vendor
                    && existing.ReadExpr == entry.ReadExpr
                    && existing.ComponentType == entry.ComponentType)
                    return;

                throw new InvalidOperationException(
                    $"Duplicate component registration for binding path '{bindingPath}': " +
                    $"existing [{existing.ComponentId}, {existing.Vendor}] vs " +
                    $"new [{entry.ComponentId}, {entry.Vendor}]. " +
                    "Each binding path must map to exactly one component.");
            }

            _componentsMap[bindingPath] = entry;
        }

        public string Render()
        {
            ResolveAll();
            return JsonSerializer.Serialize(new
            {
                planId = PlanId,
                components = SerializeComponentsMap(),
                entries = _entries
            }, CompactOptions);
        }

        public string RenderFormatted()
        {
            ResolveAll();
            return JsonSerializer.Serialize(new
            {
                planId = PlanId,
                components = SerializeComponentsMap(),
                entries = _entries
            }, FormattedOptions);
        }

        private Dictionary<string, object> SerializeComponentsMap()
        {
            var result = new Dictionary<string, object>();
            foreach (var kvp in _componentsMap)
            {
                result[kvp.Key] = new
                {
                    id = kvp.Value.ComponentId,
                    vendor = kvp.Value.Vendor,
                    readExpr = kvp.Value.ReadExpr,
                    componentType = kvp.Value.ComponentType
                };
            }
            return result;
        }

        private void ResolveAll()
        {
            var extractor = ReactivePlanConfig.Extractor;
            if (extractor != null)
            {
                ValidationResolver.Resolve(_entries, extractor, _componentsMap);
            }
            else if (ValidationResolver.HasValidatorTypes(_entries))
            {
                throw new InvalidOperationException(
                    "One or more requests use Validate<TValidator>() but no validation extractor is registered. " +
                    "Call ReactivePlanConfig.UseValidationExtractor(...) at app startup.");
            }
            else if (_componentsMap.Count > 0)
            {
                ValidationResolver.EnrichFromComponents(_entries, _componentsMap);
            }

            // Stamp planId on all validation descriptors for summary div scoping
            ValidationResolver.StampPlanId(_entries, PlanId);
        }
    }
}
