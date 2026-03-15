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

        public ReactivePlan() { }

        public string PlanId { get; } = typeof(TModel).FullName!;

        public IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap => _componentsMap;

        public void AddEntry(Entry entry)
        {
            _entries.Add(entry);
        }

        public void AddToComponentsMap(string bindingPath, ComponentRegistration entry)
        {
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
                    readExpr = kvp.Value.ReadExpr
                };
            }
            return result;
        }

        private void ResolveAll()
        {
            var extractor = ReactivePlanConfig.Extractor;
            if (extractor != null)
            {
                ValidationResolver.Resolve(_entries, extractor);
            }
            else if (ValidationResolver.HasValidatorTypes(_entries))
            {
                throw new InvalidOperationException(
                    "One or more requests use Validate<TValidator>() but no validation extractor is registered. " +
                    "Call ReactivePlanConfig.UseValidationExtractor(...) at app startup.");
            }
        }
    }
}
