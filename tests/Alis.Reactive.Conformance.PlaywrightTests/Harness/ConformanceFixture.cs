using System.Collections.Concurrent;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public abstract class ConformanceFixture(string host)
{
    private const string SyncfusionsScript = "/ej2.min.js";

    private readonly ConcurrentDictionary<string, ConcurrentBag<TestWorld>> _worlds = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentBag<IBrowserContext>> _people = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _asked = new(StringComparer.Ordinal);

    protected async Task<TestWorld> NewWorld()
    {
        var world = await TestWorld.Open(await ConformanceHost.Of(host));
        _worlds.GetOrAdd(TestContext.CurrentContext.Test.ID, _ => []).Add(world);
        Watch(world.Page);
        return world;
    }

    protected async Task<IPage> NewPerson()
    {
        var context = await TestWorld.NewContext(await ConformanceHost.Of(host));
        _people.GetOrAdd(TestContext.CurrentContext.Test.ID, _ => []).Add(context);
        var page = await context.NewPageAsync();
        Watch(page);
        return page;
    }

    [TearDown]
    public async Task EndEveryWorldOfTheTest()
    {
        var test = TestContext.CurrentContext.Test.ID;
        if (_people.TryRemove(test, out var people))
        {
            foreach (var person in people) await Waits.Close(person);
        }

        var parked = new List<string>();
        if (_worlds.TryRemove(test, out var worlds))
        {
            foreach (var world in worlds) parked.AddRange(await world.End());
        }

        var askedOfSyncfusion = _asked.TryRemove(test, out var asked) ? asked.Where(IsSyncfusionsScript).Distinct(StringComparer.Ordinal).ToList() : [];

        if (parked.Count > 0)
            throw new InvalidOperationException("The test ended with a gate still parked, so a request of its world never ran: " + string.Join(", ", parked));
        if (askedOfSyncfusion.Count > 0)
        {
            throw new InvalidOperationException(
                "A page of the test asked for Syncfusion's script, " + string.Join(", ", askedOfSyncfusion)
                + ". A core test's page holds the conformance library's components and no component library, so a change to Fusion cannot fail it and a framework defect cannot hide behind one; a Syncfusion component is tested by its own slice's tests.");
        }
    }

    private void Watch(IPage page)
    {
        var asked = _asked.GetOrAdd(TestContext.CurrentContext.Test.ID, _ => new ConcurrentQueue<string>());
        page.Request += (_, request) => asked.Enqueue(request.Url);
    }

    private static bool IsSyncfusionsScript(string address) => new Uri(address).AbsolutePath.EndsWith(SyncfusionsScript, StringComparison.Ordinal);
}
