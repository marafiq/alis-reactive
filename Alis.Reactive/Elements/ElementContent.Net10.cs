#if NET10_0_OR_GREATER
using System;

namespace Alis.Reactive.Elements;

public sealed partial class ElementContent
{
    /// <summary>Sets the content to a day when the step runs, shown as its text, yyyy-MM-dd.</summary>
    /// <param name="day">The day, written into the plan where the view renders.</param>
    public void Set(DateOnly day) => Written(Shown(day));

    /// <summary>Sets the content to a time of day when the step runs, shown as its text, HH:mm:ss.</summary>
    /// <param name="time">The time of day, written into the plan where the view renders.</param>
    public void Set(TimeOnly time) => Written(Shown(time));
}
#endif
