using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class SecondOrigin(IServer server)
{
    internal const string Policy = "the page's origin and the trace headers";

    private const int TheTracingHostsAddresses = 2;

    internal string AddressBeside(HttpRequest request)
    {
        var own = AddressOf(request);
        return Addresses().LastOrDefault(address => !IsTheSame(address, own)) ?? own;
    }

    internal bool Received(HttpRequest request)
    {
        var addresses = Addresses();
        return addresses.Count == TheTracingHostsAddresses && IsTheSame(addresses[^1], AddressOf(request));
    }

    internal bool IsThisServers(string origin) => Addresses().Any(address => IsTheSame(address, origin));

    private List<string> Addresses()
        => [.. (server.Features.Get<IServerAddressesFeature>() ?? throw new InvalidOperationException("The server lists no addresses, so the second origin cannot tell its own from the page's.")).Addresses];

    private static string AddressOf(HttpRequest request) => request.Scheme + "://" + request.Host;

    private static bool IsTheSame(string address, string other) => string.Equals(address, other, StringComparison.OrdinalIgnoreCase);
}
