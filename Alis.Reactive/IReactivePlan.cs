using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Resolvers;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    public interface IReactivePlan<TModel> where TModel : class
    {
        void AddEntry(Entry entry);
        void AddToComponentsMap(string bindingPath, ComponentRegistration entry);
        IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap { get; }
        string Render();
        string RenderFormatted();
    }

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
                ValidationResolver.Resolve(_entries, extractor);
        }
    }

    /// <summary>
    /// One-time configuration. Call at app startup.
    /// </summary>
    public static class ReactivePlanConfig
    {
        internal static IValidationExtractor? Extractor { get; private set; }

        public static void UseValidationExtractor(IValidationExtractor extractor)
        {
            Extractor = extractor;
        }
    }

    public sealed class ComponentRegistration
    {
        public string ComponentId { get; }
        public string Vendor { get; }
        public string BindingPath { get; }
        public string ReadExpr { get; }

        public ComponentRegistration(string componentId, string vendor, string bindingPath, string readExpr)
        {
            ComponentId = componentId;
            Vendor = vendor;
            BindingPath = bindingPath;
            ReadExpr = readExpr;
        }
    }
}
