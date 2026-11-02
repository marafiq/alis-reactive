using System;

namespace Alis.Reactive.Builders.Conditions
{
    // A TimeSpan reaches the browser as its text, and text orders by its characters: "1.02:00:00" (26 hours)
    // sorts before "23:00:00". The browser cannot order it as C# does, so a page that orders one, in a condition
    // or in an array lambda, is refused where it renders.
    internal static class BrowserOrder
    {
        internal static void Require(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            if (underlying == typeof(TimeSpan))
                throw new InvalidOperationException(
                    "A TimeSpan has no order the browser can compute as C# does: it reaches the browser as its text, and " +
                    "\"1.02:00:00\" sorts before \"23:00:00\". Order it on the server, or read the length as a number.");
        }
    }
}
