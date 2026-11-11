namespace Alis.Reactive.ArchitectureTests;

internal sealed class WrittenFolder : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("alis-reactive-case-").FullName;

    internal string Write(string name, string text)
    {
        var path = Path.Combine(_folder, name);
        File.WriteAllText(path, text);
        return path;
    }

    public void Dispose() => Directory.Delete(_folder, recursive: true);
}
