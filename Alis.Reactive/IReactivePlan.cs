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
        private readonly List<Entry> _entries = new List<Entry>();

        public void AddEntry(Entry entry)
        {
            _entries.Add(entry);
        }

        public string Render()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(new { entries = _entries }, options);
        }

        /// <summary>
        /// Renders the plan JSON with indentation for display purposes.
        /// </summary>
        public string RenderFormatted()
        {
            var json = Render();
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
