using Microsoft.Extensions.Caching.Distributed;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class WorldCache(IDistributedCache held, IHttpContextAccessor requests) : IDistributedCache
{
    public byte[]? Get(string key) => held.Get(InItsWorld(key));

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => held.GetAsync(InItsWorld(key), token);

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => held.Set(InItsWorld(key), value, options);

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        => held.SetAsync(InItsWorld(key), value, options, token);

    public void Refresh(string key) => held.Refresh(InItsWorld(key));

    public Task RefreshAsync(string key, CancellationToken token = default) => held.RefreshAsync(InItsWorld(key), token);

    public void Remove(string key) => held.Remove(InItsWorld(key));

    public Task RemoveAsync(string key, CancellationToken token = default) => held.RemoveAsync(InItsWorld(key), token);

    private string InItsWorld(string key)
    {
        var request = requests.HttpContext ?? throw new InvalidOperationException("The cache is read inside a request, whose world scopes it.");
        return request.RequestServices.GetRequiredService<CurrentWorld>().Key + ":" + key;
    }
}
