using System;
using System.Linq.Expressions;
using Alis.Reactive.Execution;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Declarations;

/// <summary>
/// What a declared method returned: a source of that value.
/// </summary>
/// <remarks>
/// The call runs at its line, or, read once by the next step under a later term, where that term is reached.
/// Every read keeps what the call returned, so a later step that changes the object leaves the result as it was.
/// Inside a list's lambda or a callback's answer, each invocation has its own result.
/// </remarks>
/// <typeparam name="TResult">The type the object's API gives the method's result.</typeparam>
public sealed class MethodResult<TResult> : TypedSource<TResult>
{
    private readonly BlockWriter _writer;
    private readonly ResultBinding _binding;

    internal MethodResult(ResultBinding binding, BlockWriter writer)
    {
        _binding = binding;
        _writer = writer;
    }

    internal override Schema.Expression Expression => _binding.Read();

    internal override HeldForm Form => HeldForm.Live(_writer.Content.Json, _writer.Content.RendersUnder);

    /// <summary>Reads data past the result: a field of the value the method returned, at any depth.</summary>
    /// <remarks>
    /// A field the value lacks reads as a new instance of its holder's type would hold it, and a field past an
    /// object that is null cannot be read.
    /// </remarks>
    /// <param name="path">The field to read, as a member path from the result.</param>
    /// <returns>A source of the field's value.</returns>
    public TypedSource<TRead> Read<TRead>(Expression<Func<TResult, TRead>> path)
        => DataReads.Past<TRead>(Expression, Form, path, _writer.Content.Json.LiveForm);
}
