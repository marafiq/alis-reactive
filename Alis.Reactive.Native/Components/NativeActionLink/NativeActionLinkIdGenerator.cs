#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    internal static class NativeActionLinkIdGenerator
    {
        private const string CounterKeyPrefix = "__alis_native_action_link_counter__";

        internal static string Next<TModel>(ViewContext viewContext) where TModel : class
        {
            var scope = IdGenerator.TypeScope(typeof(TModel));
            var counterKey = CounterKeyPrefix + scope;
            var next = 1;

#if NET48
            if (viewContext.HttpContext.Items.Contains(counterKey) && viewContext.HttpContext.Items[counterKey] is int current)
#else
            if (viewContext.HttpContext.Items.TryGetValue(counterKey, out var value) && value is int current)
#endif
            {
                next = current + 1;
            }

            viewContext.HttpContext.Items[counterKey] = next;
            return scope + "__native_action_link_" + next;
        }
    }
}
