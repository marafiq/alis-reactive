using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    public interface IReactivePlan<TModel> where TModel : class
    {
        void AddEntry(Entry entry);
        void RegisterComponent(string componentId, string vendor, string bindingPath, string readExpr);
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
        private readonly List<ComponentRegistration> _components = new List<ComponentRegistration>();
        private readonly IValidationExtractor? _extractor;

        public ReactivePlan() : this(null) { }

        public ReactivePlan(IValidationExtractor? extractor)
        {
            _extractor = extractor;
        }

        public void AddEntry(Entry entry)
        {
            _entries.Add(entry);
        }

        public void RegisterComponent(string componentId, string vendor, string bindingPath, string readExpr)
        {
            _components.Add(new ComponentRegistration(componentId, vendor, bindingPath, readExpr));
        }

        public string Render()
        {
            ResolveAllGather();
            ResolveAllValidation();
            return JsonSerializer.Serialize(new { entries = _entries }, CompactOptions);
        }

        public string RenderFormatted()
        {
            ResolveAllGather();
            ResolveAllValidation();
            return JsonSerializer.Serialize(new { entries = _entries }, FormattedOptions);
        }

        private void ResolveAllGather()
        {
            foreach (var entry in _entries)
            {
                ResolveGatherInReaction(entry.Reaction);
            }
        }

        private void ResolveGatherInReaction(Reaction reaction)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveGatherInRequest(hr.Request);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveGatherInRequest(req);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveGatherInReaction(branch.Reaction);
                    break;
            }
        }

        private void ResolveGatherInRequest(RequestDescriptor req)
        {
            if (req.Gather != null)
            {
                var expanded = new List<GatherItem>();
                foreach (var item in req.Gather)
                {
                    if (item is AllGather ag && ag.FormId == null)
                    {
                        foreach (var c in _components)
                        {
                            expanded.Add(new ComponentGather(c.ComponentId, c.Vendor, c.BindingPath, c.ReadExpr));
                        }
                    }
                    else
                    {
                        expanded.Add(item);
                    }
                }
                req.Gather = expanded;
            }

            if (req.Chained != null)
            {
                ResolveGatherInRequest(req.Chained);
            }
        }

        private void ResolveAllValidation()
        {
            if (_extractor == null) return;

            foreach (var entry in _entries)
            {
                ResolveReaction(entry.Reaction);
            }
        }

        private void ResolveReaction(Reaction reaction)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveRequest(hr.Request);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveRequest(req);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveReaction(branch.Reaction);
                    break;
            }
        }

        private void ResolveRequest(RequestDescriptor req)
        {
            if (req.ValidatorType != null && req.Validation != null)
            {
                var formId = req.Validation.FormId;
                var extracted = _extractor!.ExtractRules(req.ValidatorType, formId);
                if (extracted != null)
                {
                    req.Validation = extracted;
                }
            }

            if (req.ReadExprOverrides != null && req.Validation != null)
            {
                foreach (var kvp in req.ReadExprOverrides)
                {
                    req.Validation = req.Validation.WithReadExpr(kvp.Key, kvp.Value);
                }
            }

            if (req.Chained != null)
            {
                ResolveRequest(req.Chained);
            }
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
