using System.Collections.Generic;
using Alis.Reactive.Descriptors;

namespace Alis.Reactive
{
    public interface IReactivePlan<TModel> where TModel : class
    {
        string PlanId { get; }
        bool IsPartial { get; }
        void AddEntry(Entry entry);
        void AddToComponentsMap(string bindingPath, ComponentRegistration entry);
        IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap { get; }
        string Render();
        string RenderFormatted();
    }
}
