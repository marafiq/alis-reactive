using System;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The MVC field prefix a view renders under (<c>TemplateInfo.HtmlFieldPrefix</c>): a partial rendered for a
    /// member of the page's model binds its inputs under the member's name. Every binding name and input id
    /// a plan derives from a member expression carries it, so a partial rendered twice under two prefixes
    /// renders two inputs, gathers its own and checks its own.
    /// </summary>
    internal static class HtmlFieldPrefix
    {
        internal const string None = "";

        /// <summary>A member path under the prefix: <c>NextOfKin</c> and <c>Name</c> make <c>NextOfKin.Name</c>.</summary>
        internal static string Join(string prefix, string path)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (path == null) throw new ArgumentNullException(nameof(path));
            return prefix.Length == 0 ? path : path.Length == 0 ? prefix : prefix + "." + path;
        }

        /// <summary>A member's binding under the prefix: its MVC name and its body path both start with it.</summary>
        internal static BindingPath Join(string prefix, BindingPath member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return prefix.Length == 0 ? member : BindingPath.ForMember(Join(prefix, member.Value), Join(prefix, member.Path));
        }

        /// <summary>A body path under the prefix: the prefix's own steps, then the member's.</summary>
        internal static Path Join(string prefix, Path body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            return prefix.Length == 0 ? body : Path.Parse(prefix).Append(body);
        }

        /// <summary>The path under the prefix, relative to it; null for a path of another prefix.</summary>
        internal static string? Strip(string prefix, string path)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (prefix.Length == 0) return path;
            if (string.Equals(path, prefix, StringComparison.Ordinal)) return "";
            return path.StartsWith(prefix + ".", StringComparison.Ordinal) ? path.Substring(prefix.Length + 1) : null;
        }
    }
}
