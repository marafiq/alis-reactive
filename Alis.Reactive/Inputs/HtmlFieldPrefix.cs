using System;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.Inputs
{
    internal static class HtmlFieldPrefix
    {
        internal const string None = "";

        internal static string Join(string prefix, string path)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (path == null) throw new ArgumentNullException(nameof(path));
            return prefix.Length == 0 ? path : path.Length == 0 ? prefix : prefix + "." + path;
        }

        internal static BindingPath Join(string prefix, BindingPath member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return prefix.Length == 0 ? member : BindingPath.ForMember(Join(prefix, member.Value), Join(prefix, member.Path));
        }

        internal static Path Join(string prefix, Path body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            return prefix.Length == 0 ? body : Path.Parse(prefix).Append(body);
        }
    }
}
