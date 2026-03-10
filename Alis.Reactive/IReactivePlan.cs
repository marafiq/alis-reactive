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

        public string Render()
        {
            ResolveAllValidation();
            return JsonSerializer.Serialize(new { entries = _entries }, CompactOptions);
        }

        public string RenderFormatted()
        {
            ResolveAllValidation();
            return JsonSerializer.Serialize(new { entries = _entries }, FormattedOptions);
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
                    if (!string.IsNullOrEmpty(req.ValidationPrefix))
                    {
                        extracted = extracted.WithPrefix(formId, req.ValidationPrefix);
                    }
                    req.Validation = extracted;
                }
            }

            if (req.Chained != null)
            {
                ResolveRequest(req.Chained);
            }
        }
    }
}
