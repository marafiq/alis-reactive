using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors;

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

        public void AddEntry(Entry entry)
        {
            _entries.Add(entry);
        }

        public string Render()
        {
            return JsonSerializer.Serialize(new { entries = _entries }, CompactOptions);
        }

        /// <summary>
        /// Renders the plan JSON with indentation for display purposes.
        /// </summary>
        public string RenderFormatted()
        {
            return JsonSerializer.Serialize(new { entries = _entries }, FormattedOptions);
        }
    }
}
