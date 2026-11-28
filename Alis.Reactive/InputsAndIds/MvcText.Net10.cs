#if NET10_0_OR_GREATER
using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Alis.Reactive.InputsAndIds;

internal static partial class MvcText
{
    private const string DotReplacement = "_";

    private static readonly ModelExpressionProvider Expressions = new(new EmptyModelMetadataProvider());

    internal static partial string Of<TModel, TValue>(Expression<Func<TModel, TValue>> member) => Expressions.GetExpressionText(member);

    internal static partial string IdFromName(string name) => TagBuilder.CreateSanitizedId(name, DotReplacement);
}
#endif
