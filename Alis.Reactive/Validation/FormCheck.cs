using System.Collections.Generic;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Validation;

internal sealed class FormCheck
{
    internal FormCheck(string registration, CheckedBody body, IReadOnlyList<SentInput> inputs, FormPlace place)
    {
        Registration = registration;
        Body = body;
        Inputs = inputs;
        Place = place;
    }

    public string Registration { get; }

    public CheckedBody Body { get; }

    public IReadOnlyList<SentInput> Inputs { get; }

    public FormPlace Place { get; }
}

internal sealed class CheckedBody
{
    internal CheckedBody(BodyFormat format, EvaluatedValue value)
    {
        Format = format;
        Value = value;
    }

    public BodyFormat Format { get; }

    public EvaluatedValue Value { get; }
}
