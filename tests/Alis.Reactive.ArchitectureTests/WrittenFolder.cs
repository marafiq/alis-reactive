namespace Alis.Reactive.ArchitectureTests;

internal sealed class WrittenFolder : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("alis-reactive-case-").FullName;

    internal string Write(string name, string text)
    {
        var path = Path.Combine(_folder, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
        return path;
    }

    internal string Write(string name, byte[] bytes)
    {
        var path = Path.Combine(_folder, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    public void Dispose() => Directory.Delete(_folder, recursive: true);
}
