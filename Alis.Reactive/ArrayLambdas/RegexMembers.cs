using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Alis.Reactive.ArrayLambdas;

[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's Decision 2, 2026-11-06: Regex.IsMatch in a list's lambda is keyed in the table of translated members, the ban's one exception; the core runs no Regex, and the pattern passes D16.11's one reader (D6.15).")]
internal static class RegexMembers
{
    internal static Expression<Func<bool>> IsMatch => () => Regex.IsMatch("", "");

    internal static Expression<Func<bool>> IsMatchWithOptions => () => Regex.IsMatch("", "", RegexOptions.None);
}
