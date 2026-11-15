using System.Collections.Generic;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal static class TextForms
{
    private const string Join = "join";

    internal static AsExpression Shown<T>(ITypedSource<T> source) => new(Sources.Of(source), StringShape.Instance);

    internal static LitExpression Part(string? text) => Literals.Text(text ?? string.Empty);

    internal static OpExpression Part<T>(ITypedSource<T> source) => new(Operator.Coalesce, [Shown(source), Literals.Text(string.Empty)]);

    internal static CallExpression Joined(IReadOnlyList<Expression> parts)
        => new([new MemberStep(Join)], PathMembers.RequiredIfHeld, new ListExpression(parts), [Literals.Text(string.Empty)]);
}
