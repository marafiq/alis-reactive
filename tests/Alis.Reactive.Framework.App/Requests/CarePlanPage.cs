namespace Alis.Reactive.Framework.App.Requests;

/// <summary>A resident's care plan, chosen as a file: the page reads what the browser knows of it, and sends it.</summary>
public sealed class CarePlanPage
{
    public List<IFormFile>? CarePlan { get; set; }
}

/// <summary>What the page read of the chosen care plan, posted as JSON before the file itself is sent.</summary>
public sealed class CarePlanCheck
{
    public BrowserFile? CarePlan { get; set; }
}

/// <summary>The care plan bound as the files of a sequence: a base type of the page's list.</summary>
public sealed class CarePlanUpload
{
    public IEnumerable<IFormFile>? CarePlan { get; set; }
}

/// <summary>The care plan bound as an array of files: another collection of the same items as the page's list.</summary>
public sealed class CarePlanFiles
{
    public IFormFile[]? CarePlan { get; set; }
}

/// <summary>The resident's signed consents, chosen as files the page's model holds as a sequence.</summary>
public sealed class ConsentsPage
{
    public IEnumerable<IFormFile>? Consents { get; set; }
}

/// <summary>The consents bound as an array of files: another collection of the same items as the page's sequence.</summary>
public sealed class ConsentFiles
{
    public IFormFile[]? Consents { get; set; }
}
