using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Resolvers;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    public interface IReactivePlan<TModel> where TModel : class
    {
        void AddEntry(Entry entry);
        void RegisterComponent(string componentId, string vendor, string bindingPath, string readExpr);
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
        private readonly Dictionary<RequestDescriptor, RequestBuildContext> _buildContexts = new Dictionary<RequestDescriptor, RequestBuildContext>();
        private readonly IValidationExtractor? _extractor;

        public ReactivePlan() : this(null) { }

        public ReactivePlan(IValidationExtractor? extractor)
        {
            _extractor = extractor;
        }

        public IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap => _componentsMap;

        public void AddEntry(Entry entry)
        {
            _entries.Add(entry);
        }

        public void RegisterComponent(string componentId, string vendor, string bindingPath, string readExpr)
        {
            _componentsMap[bindingPath] = new ComponentRegistration(componentId, vendor, bindingPath, readExpr);
        }

        public void AddToComponentsMap(string bindingPath, ComponentRegistration entry)
        {
            _componentsMap[bindingPath] = entry;
        }

        internal void RegisterBuildContexts(Dictionary<RequestDescriptor, RequestBuildContext>? contexts)
        {
            if (contexts == null) return;
            foreach (var kvp in contexts)
                _buildContexts[kvp.Key] = kvp.Value;
        }

        public string Render()
        {
            ResolveAll();
            return JsonSerializer.Serialize(new { entries = _entries }, CompactOptions);
        }

        public string RenderFormatted()
        {
            ResolveAll();
            return JsonSerializer.Serialize(new { entries = _entries }, FormattedOptions);
        }

        private void ResolveAll()
        {
            GatherResolver.Resolve(_entries, _componentsMap);

            if (_extractor != null)
                ValidationResolver.Resolve(_entries, _extractor, _buildContexts);
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
