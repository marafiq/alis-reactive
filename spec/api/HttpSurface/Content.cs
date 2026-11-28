using System;

namespace Alis.Reactive.Requests;

public static class Content
{
    public static NoContent None => throw new NotSupportedException();

    public static ErrorContent<string> Text => throw new NotSupportedException();

    public static ErrorContent<Markup> Html => throw new NotSupportedException();

    public static SuccessContent<ResponseFile> File => throw new NotSupportedException();

    public static ErrorContent<TBody> Json<TBody>() => throw new NotSupportedException();
}

public sealed class NoContent
{
    private NoContent()
    {
    }
}

public abstract class SuccessContent<TBody>
{
    private protected SuccessContent()
    {
    }
}

public sealed class ErrorContent<TBody> : SuccessContent<TBody>
{
    private ErrorContent()
    {
    }
}
