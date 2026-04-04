using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native
{
    internal static class NativeEventPaths
    {
        internal static List<PathSegment> FromCurrentTarget(IReadOnlyList<PathSegment> bindingPath)
        {
            var eventPath = new List<PathSegment> { PathSegment.FromProp("currentTarget") };
            eventPath.AddRange(CapabilityPath.Clone(bindingPath));
            return eventPath;
        }
    }
}
