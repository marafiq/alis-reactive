namespace Alis.Reactive.Framework.App.Requests;

public sealed class CarePlanPage
{
    public List<IFormFile>? CarePlan { get; set; }
}

public sealed class CarePlanCheck
{
    public BrowserFile? CarePlan { get; set; }
}

public sealed class CarePlanUpload
{
    public IEnumerable<IFormFile>? CarePlan { get; set; }
}

public sealed class CarePlanFiles
{
    public IFormFile[]? CarePlan { get; set; }
}

public sealed class ListedCarePlan
{
    public IReadOnlyList<BrowserFile>? CarePlan { get; set; }
}

public sealed class ConsentsPage
{
    public IEnumerable<IFormFile>? Consents { get; set; }
}

public sealed class ConsentFiles
{
    public IFormFile[]? Consents { get; set; }
}
