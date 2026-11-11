using System.Collections.Concurrent;
using NUnit.Framework;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public abstract class ConformanceFixture(string host)
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<TestWorld>> _worlds = new(StringComparer.Ordinal);

    protected async Task<TestWorld> NewWorld()
    {
        var world = await TestWorld.Open(await ConformanceHost.Of(host));
        _worlds.GetOrAdd(TestContext.CurrentContext.Test.ID, _ => []).Add(world);
        return world;
    }

    [TearDown]
    public async Task EndEveryWorldOfTheTest()
    {
        if (!_worlds.TryRemove(TestContext.CurrentContext.Test.ID, out var worlds)) return;

        var parked = new List<string>();
        foreach (var world in worlds) parked.AddRange(await world.End());
        if (parked.Count > 0)
            throw new InvalidOperationException("The test ended with a gate still parked, so a request of its world never ran: " + string.Join(", ", parked));
    }
}
