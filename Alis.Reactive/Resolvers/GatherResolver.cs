using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Resolvers
{
    /// <summary>
    /// Walks the reaction tree and expands AllGather markers into explicit ComponentGather items.
    /// </summary>
    internal static class GatherResolver
    {
        internal static void Resolve(List<Entry> entries, IReadOnlyDictionary<string, ComponentRegistration> componentsMap)
        {
            foreach (var entry in entries)
            {
                ResolveReaction(entry.Reaction, componentsMap);
            }
        }

        private static void ResolveReaction(Reaction reaction, IReadOnlyDictionary<string, ComponentRegistration> componentsMap)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveRequest(hr.Request, componentsMap);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveRequest(req, componentsMap);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveReaction(branch.Reaction, componentsMap);
                    break;
            }
        }

        private static void ResolveRequest(RequestDescriptor req, IReadOnlyDictionary<string, ComponentRegistration> componentsMap)
        {
            if (req.Gather != null && req.Gather.Exists(item => item is AllGather))
            {
                var expanded = new List<GatherItem>();
                foreach (var item in req.Gather)
                {
                    if (item is AllGather)
                    {
                        foreach (var c in componentsMap.Values)
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
                ResolveRequest(req.Chained, componentsMap);
            }
        }
    }
}
