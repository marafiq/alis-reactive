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
        internal static void Resolve(List<Entry> entries, IReadOnlyList<ComponentRegistration> components)
        {
            foreach (var entry in entries)
            {
                ResolveReaction(entry.Reaction, components);
            }
        }

        private static void ResolveReaction(Reaction reaction, IReadOnlyList<ComponentRegistration> components)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveRequest(hr.Request, components);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveRequest(req, components);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveReaction(branch.Reaction, components);
                    break;
            }
        }

        private static void ResolveRequest(RequestDescriptor req, IReadOnlyList<ComponentRegistration> components)
        {
            if (req.Gather != null && req.Gather.Exists(item => item is AllGather))
            {
                var expanded = new List<GatherItem>();
                foreach (var item in req.Gather)
                {
                    if (item is AllGather)
                    {
                        foreach (var c in components)
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
                ResolveRequest(req.Chained, components);
            }
        }
    }
}
