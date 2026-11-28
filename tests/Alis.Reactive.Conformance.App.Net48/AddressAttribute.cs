using System.Web.Mvc.Routing;

namespace Alis.Reactive.Conformance.App;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AddressAttribute(string address) : RouteFactoryAttribute(address.TrimStart('/'));
