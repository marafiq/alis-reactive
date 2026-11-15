using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

if (args.Length != 3)
{
    Console.Error.WriteLine("usage: dotnet run scripts/page-doors.cs -- <the app's assembly> <its Playwright tests' assembly> <the table to write>");
    Console.Error.WriteLine("Reads a Release build of an app and its Playwright tests, the framework's assemblies beside the app's, and writes to the table, for each test's promise, the views its pages render and the modules of the framework's members those views call that throw NotImplementedException, the doors not built.");
    return 2;
}

var (appPath, testsPath, table) = (args[0], args[1], args[2]);
var appFolder = Path.GetDirectoryName(Path.GetFullPath(appPath))!;
string[] inputs = [appPath, testsPath, .. Framework.Names.Select(name => Path.Combine(appFolder, name + ".dll"))];
var missing = inputs.Where(path => !File.Exists(path)).ToList();
if (missing.Count > 0)
{
    Console.Error.WriteLine("[page-doors] the build holds no " + string.Join(", no ", missing) + "; build Alis.Reactive.slnx -c Release first.");
    return 2;
}

var app = new Loaded(appPath);
var tests = new Loaded(testsPath);
var framework = new Framework(Framework.Names.ToDictionary(name => name, name => new Loaded(Path.Combine(appFolder, name + ".dll"))));
var pages = new Pages(app, framework);
var problems = new List<string>();
var landed = new SortedDictionary<string, Landing>(StringComparer.Ordinal);

foreach (var (promise, method) in Promised.Tests(tests))
{
    var reached = Walk.From(tests, [method], type => !Meta.NamespaceOf(tests.Reader, type).StartsWith(Promised.Harness, StringComparison.Ordinal), null);
    var opened = pages.Opened(reached.Strings);
    if (opened.Count == 0)
    {
        problems.Add($"{promise}: its test names no route an action of the app answers a GET on, so no page of it is read");
        continue;
    }
    var landing = pages.Read(promise, opened, reached.Strings);
    landed[promise] = landed.TryGetValue(promise, out var earlier) ? earlier.With(landing) : landing;
}

problems.AddRange(pages.Problems);
problems.AddRange(framework.Problems);
if (problems.Count > 0)
{
    foreach (var problem in problems.Distinct().Order(StringComparer.Ordinal)) Console.Error.WriteLine("[page-doors] " + problem);
    return 1;
}

File.WriteAllText(table, Landing.Json(landed));
Console.WriteLine($"[page-doors] the doors the built pages of {landed.Count} promises call, written to {table}");
return 0;

internal sealed record Landing(ImmutableArray<string> Modules, ImmutableArray<string> Views)
{
    public static string Json(IReadOnlyDictionary<string, Landing> landed)
    {
        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true, NewLine = "\n" }))
        {
            json.WriteStartObject();
            foreach (var (promise, landing) in landed)
            {
                json.WriteStartObject(promise);
                Listed(json, "modules", landing.Modules);
                Listed(json, "views", landing.Views);
                json.WriteEndObject();
            }
            json.WriteEndObject();
        }
        return Encoding.UTF8.GetString(buffer.ToArray()) + "\n";
    }

    public Landing With(Landing other) => new(
        [.. Modules.Union(other.Modules).Order(StringComparer.Ordinal)],
        [.. Views.Union(other.Views).Order(StringComparer.Ordinal)]);

    private static void Listed(Utf8JsonWriter json, string name, ImmutableArray<string> items)
    {
        json.WriteStartArray(name);
        foreach (var item in items) json.WriteStringValue(item);
        json.WriteEndArray();
    }
}

internal sealed class Loaded
{
    private readonly PEReader _image;

    public Loaded(string path)
    {
        _image = new PEReader(File.OpenRead(path));
        Reader = _image.GetMetadataReader();
        Name = Reader.GetString(Reader.GetAssemblyDefinition().Name);
        Types = Reader.TypeDefinitions.ToDictionary(type => Meta.NameOf(Reader, type), type => type, StringComparer.Ordinal);
    }

    public MetadataReader Reader { get; }

    public string Name { get; }

    public IReadOnlyDictionary<string, TypeDefinitionHandle> Types { get; }

    public byte[] Il(MethodDefinitionHandle method)
    {
        var address = Reader.GetMethodDefinition(method).RelativeVirtualAddress;
        return address == 0 ? [] : _image.GetMethodBody(address).GetILBytes() ?? [];
    }
}

internal sealed class Framework(IReadOnlyDictionary<string, Loaded> assemblies)
{
    public static readonly string[] Names = ["Alis.Reactive", "Alis.Reactive.Native", "Alis.Reactive.Fusion"];

    private const string NotBuilt = "System.NotImplementedException";

    private readonly Dictionary<(string Assembly, string Type, string Member), string?> _modules = [];

    public List<string> Problems { get; } = [];

    public bool Holds(string assembly) => assemblies.ContainsKey(assembly);

    public string? ModuleOfUnbuilt(string assembly, string type, string member, string signature)
    {
        var key = (assembly, type, member + signature);
        if (_modules.TryGetValue(key, out var known)) return known;
        var module = Resolve(assemblies[assembly], type, member, signature);
        _modules[key] = module;
        return module;
    }

    private string? Resolve(Loaded holder, string type, string member, string signature)
    {
        if (!holder.Types.TryGetValue(type, out var declared))
        {
            Problems.Add($"{holder.Name} declares no type {type}, which the app calls {member} on: the app is built against another {holder.Name}, so build the solution again");
            return null;
        }
        var names = new Names(holder.Reader);
        foreach (var handle in holder.Reader.GetTypeDefinition(declared).GetMethods())
        {
            var method = holder.Reader.GetMethodDefinition(handle);
            if (holder.Reader.GetString(method.Name) != member || Meta.Key(method.DecodeSignature(names, null)) != signature) continue;
            if (!Throws(holder, handle)) return null;
            var module = Meta.ModuleOf(holder.Reader, declared, holder.Name);
            if (module.Length == 0) Problems.Add($"{type}.{member} throws {NotBuilt} in {holder.Name}'s root namespace, which names no module");
            return module.Length == 0 ? null : module;
        }
        Problems.Add($"{type} in {holder.Name} declares no {member} of the signature the app calls: the app is built against another {holder.Name}, so build the solution again");
        return null;
    }

    private static bool Throws(Loaded holder, MethodDefinitionHandle method)
    {
        var il = holder.Il(method);
        if (il.Length != 6 || il[0] != (byte)ILOpCode.Newobj || il[5] != (byte)ILOpCode.Throw) return false;
        var made = MetadataTokens.EntityHandle(BitConverter.ToInt32(il, 1));
        if (made.Kind != HandleKind.MemberReference) return false;
        var parent = holder.Reader.GetMemberReference((MemberReferenceHandle)made).Parent;
        return parent.Kind == HandleKind.TypeReference && Meta.NameOf(holder.Reader, (TypeReferenceHandle)parent) == NotBuilt;
    }
}

internal sealed class Pages
{
    private const string Mvc = "Microsoft.AspNetCore.Mvc.";
    private const string Controller = Mvc + "Controller";
    private const string ViewSuffix = ".cshtml";
    private static readonly string[] RouteAttributes = ["HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch", "HttpHead", "HttpOptions"];

    private readonly Loaded _app;
    private readonly Framework _framework;
    private readonly Dictionary<string, TypeDefinitionHandle> _views = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<Action>> _routes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Walked> _walkedViews = new(StringComparer.Ordinal);

    public Pages(Loaded app, Framework framework)
    {
        _app = app;
        _framework = framework;
        ReadCompiledViews();
        ReadActions();
    }

    public List<string> Problems { get; } = [];

    public List<string> Opened(IReadOnlySet<string> named)
        => [.. named.SelectMany(Answering).Where(action => action.Get).SelectMany(action => action.Views).Distinct()];

    public Landing Read(string promise, IEnumerable<string> opened, IReadOnlySet<string> named)
    {
        var views = new SortedSet<string>(StringComparer.Ordinal);
        var modules = new SortedSet<string>(StringComparer.Ordinal);
        var waiting = new Queue<string>(opened);
        while (waiting.TryDequeue(out var view))
        {
            if (!views.Add(view)) continue;
            if (!_views.ContainsKey(view))
            {
                Problems.Add($"{promise}: its pages name {view}, no view the app compiles");
                continue;
            }
            var walked = WalkedView(view);
            modules.UnionWith(walked.Modules);
            foreach (var text in walked.Unaddressed.Where(named.Contains))
                Problems.Add($"{promise}: its test names the link '{text}' of {view}, whose address is written by no route before it, so the page it opens is not read");
            foreach (var next in walked.Strings.Where(text => text.EndsWith(ViewSuffix, StringComparison.Ordinal) && !IsItsOwnSource(view, text))) waiting.Enqueue(next);
            var slotted = walked.Strings.SelectMany(Answering).Where(action => action.Partial);
            var linked = walked.Links.Where(link => named.Contains(link.Text)).SelectMany(link => Answering(link.Route)).Where(action => action.Get);
            foreach (var next in slotted.Concat(linked).SelectMany(action => action.Views)) waiting.Enqueue(next);
        }
        return new Landing([.. modules], [.. views]);
    }

    private bool IsItsOwnSource(string view, string text)
        => !_views.ContainsKey(text) && text.Length > view.Length && text.EndsWith(view, StringComparison.Ordinal);

    private IEnumerable<Action> Answering(string url)
    {
        if (!url.StartsWith('/')) return [];
        var path = url.Split('?')[0];
        if (_routes.TryGetValue(path, out var exact)) return exact;
        var segments = path.Split('/');
        return _routes.Where(route => route.Key.Contains('{') && Fills(route.Key.Split('/'), segments)).SelectMany(route => route.Value);
    }

    private static bool Fills(string[] template, string[] segments)
        => template.Length == segments.Length && template.Zip(segments).All(pair => pair.First == pair.Second || (pair.First.StartsWith('{') && pair.First.EndsWith('}') && pair.Second.Length > 0));

    private Walked WalkedView(string view)
    {
        if (_walkedViews.TryGetValue(view, out var walked)) return walked;
        var type = _views[view];
        walked = Walk.From(_app, _app.Reader.GetTypeDefinition(type).GetMethods(), _ => true, _framework);
        _walkedViews[view] = walked;
        return walked;
    }

    private void ReadCompiledViews()
    {
        foreach (var handle in _app.Reader.GetAssemblyDefinition().GetCustomAttributes())
        {
            var attribute = _app.Reader.GetCustomAttribute(handle);
            if (Meta.AttributeName(_app.Reader, attribute) != "Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute") continue;
            var value = _app.Reader.GetBlobReader(attribute.Value);
            value.ReadUInt16();
            var typeName = value.ReadSerializedString() ?? "";
            value.ReadSerializedString();
            var path = value.ReadSerializedString() ?? "";
            var simple = typeName.Split(',')[0].Trim();
            if (_app.Types.TryGetValue(simple, out var type)) _views[path] = type;
            else Problems.Add($"the compiled view {path} names {simple}, a type the app does not hold");
        }
    }

    private void ReadActions()
    {
        foreach (var typeHandle in _app.Reader.TypeDefinitions)
        {
            foreach (var methodHandle in _app.Reader.GetTypeDefinition(typeHandle).GetMethods())
            {
                foreach (var attributeHandle in _app.Reader.GetMethodDefinition(methodHandle).GetCustomAttributes())
                {
                    var attribute = _app.Reader.GetCustomAttribute(attributeHandle);
                    var name = Meta.AttributeName(_app.Reader, attribute);
                    var verb = RouteAttributes.FirstOrDefault(each => name == Mvc + each + "Attribute");
                    if (verb is null || Meta.ParameterCount(_app.Reader, attribute) != 1) continue;
                    var value = _app.Reader.GetBlobReader(attribute.Value);
                    value.ReadUInt16();
                    var route = value.ReadSerializedString() ?? "";
                    var walked = Walk.From(_app, [methodHandle], _ => true, null);
                    var views = walked.Strings.Where(text => text.EndsWith(ViewSuffix, StringComparison.Ordinal)).ToList();
                    var rendersAView = walked.Calls.Contains(Controller + ".View") || walked.Calls.Contains(Controller + ".PartialView");
                    if (rendersAView && views.Count == 0)
                        Problems.Add($"{Meta.NameOf(_app.Reader, typeHandle)}.{_app.Reader.GetString(_app.Reader.GetMethodDefinition(methodHandle).Name)} renders a view it names by no path");
                    var action = new Action(verb == "HttpGet", walked.Calls.Contains(Controller + ".PartialView"), views);
                    if (!_routes.TryGetValue(route, out var actions)) _routes[route] = actions = [];
                    actions.Add(action);
                }
            }
        }
    }

    private sealed record Action(bool Get, bool Partial, List<string> Views);
}

internal sealed record Walked(IReadOnlySet<string> Strings, IReadOnlySet<string> Calls, IReadOnlySet<string> Modules, IReadOnlyList<(string Text, string Route)> Links, IReadOnlySet<string> Unaddressed);

internal static class Walk
{
    public static Walked From(Loaded module, IEnumerable<MethodDefinitionHandle> starts, Func<TypeDefinitionHandle, bool> follows, Framework? framework)
    {
        var reader = module.Reader;
        var strings = new HashSet<string>(StringComparer.Ordinal);
        var calls = new HashSet<string>(StringComparer.Ordinal);
        var modules = new HashSet<string>(StringComparer.Ordinal);
        var links = new List<(string, string)>();
        var unaddressed = new HashSet<string>(StringComparer.Ordinal);
        var seen = new HashSet<MethodDefinitionHandle>();
        var taken = new HashSet<TypeDefinitionHandle>();
        var initialized = new HashSet<TypeDefinitionHandle>();
        var waiting = new Queue<MethodDefinitionHandle>(starts);
        var names = new Names(reader);

        void Initialized(TypeDefinitionHandle type)
        {
            if (!follows(type) || !initialized.Add(type)) return;
            foreach (var method in reader.GetTypeDefinition(type).GetMethods().Where(method => reader.GetString(reader.GetMethodDefinition(method).Name) == ".cctor")) waiting.Enqueue(method);
        }

        void Take(TypeDefinitionHandle type)
        {
            if (!follows(type) || !taken.Add(type)) return;
            Initialized(type);
            var definition = reader.GetTypeDefinition(type);
            foreach (var method in definition.GetMethods().Where(method => Meta.TheFrameworkCanCall(reader, method))) waiting.Enqueue(method);
            if (!definition.BaseType.IsNil && definition.BaseType.Kind == HandleKind.TypeDefinition) Take((TypeDefinitionHandle)definition.BaseType);
        }

        void TakeArguments(EntityHandle parent)
        {
            if (parent.Kind != HandleKind.TypeSpecification) return;
            var arguments = new HashSet<TypeDefinitionHandle>();
            reader.GetTypeSpecification((TypeSpecificationHandle)parent).DecodeSignature(new Names(reader, arguments), null);
            foreach (var argument in arguments) Take(argument);
        }

        void Touched(EntityHandle field)
        {
            if (field.Kind == HandleKind.FieldDefinition) Initialized(reader.GetFieldDefinition((FieldDefinitionHandle)field).GetDeclaringType());
            if (field.Kind != HandleKind.MemberReference) return;
            var parent = reader.GetMemberReference((MemberReferenceHandle)field).Parent;
            TakeArguments(parent);
            if (Meta.ParentOf(reader, parent, module.Name).Own is { } local) Initialized(local);
        }

        void Called(EntityHandle target, bool made)
        {
            switch (target.Kind)
            {
                case HandleKind.MethodDefinition:
                    var own = (MethodDefinitionHandle)target;
                    var declaring = reader.GetMethodDefinition(own).GetDeclaringType();
                    if (follows(declaring)) waiting.Enqueue(own);
                    Initialized(declaring);
                    if (made) Take(declaring);
                    break;
                case HandleKind.MethodSpecification:
                    var specification = reader.GetMethodSpecification((MethodSpecificationHandle)target);
                    var arguments = new HashSet<TypeDefinitionHandle>();
                    specification.DecodeSignature(new Names(reader, arguments), null);
                    foreach (var argument in arguments) Take(argument);
                    Called(specification.Method, made);
                    break;
                case HandleKind.MemberReference:
                    Referenced((MemberReferenceHandle)target, made);
                    break;
            }
        }

        void Referenced(MemberReferenceHandle handle, bool made)
        {
            var member = reader.GetMemberReference(handle);
            if (member.GetKind() != MemberReferenceKind.Method) return;
            var (assembly, type, ownType) = Meta.ParentOf(reader, member.Parent, module.Name);
            var name = reader.GetString(member.Name);
            var signature = Meta.Key(member.DecodeMethodSignature(names, null));
            calls.Add(type + "." + name);
            TakeArguments(member.Parent);
            if (ownType is { } local)
            {
                Initialized(local);
                if (made) Take(local);
                if (!follows(local)) return;
                foreach (var method in reader.GetTypeDefinition(local).GetMethods().Where(method => reader.GetString(reader.GetMethodDefinition(method).Name) == name && Meta.Key(reader.GetMethodDefinition(method).DecodeSignature(names, null)) == signature)) waiting.Enqueue(method);
                return;
            }
            if (framework is null || !framework.Holds(assembly)) return;
            if (framework.ModuleOfUnbuilt(assembly, type, name, signature) is { } unbuilt) modules.Add(unbuilt);
        }

        while (waiting.TryDequeue(out var method))
        {
            if (!seen.Add(method)) continue;
            string? route = null;
            foreach (var (code, token) in Il.Read(module.Il(method)))
            {
                switch (code)
                {
                    case ILOpCode.Ldstr:
                        var text = reader.GetUserString(MetadataTokens.UserStringHandle(token));
                        strings.Add(text);
                        var closed = text.IndexOf(AnchorEnd, StringComparison.Ordinal);
                        var anchor = text.StartsWith('>') && closed > 1 && closed == text.IndexOf('<') ? text[1..closed].Trim() : null;
                        if (anchor is not null && route is not null) links.Add((anchor, route));
                        if (anchor is not null && route is null) unaddressed.Add(anchor);
                        route = text.StartsWith('/') ? text : route is not null && text.Length == 0 ? route : null;
                        break;
                    case ILOpCode.Call or ILOpCode.Callvirt or ILOpCode.Ldftn or ILOpCode.Ldvirtftn:
                        Called(MetadataTokens.EntityHandle(token), false);
                        break;
                    case ILOpCode.Newobj:
                        Called(MetadataTokens.EntityHandle(token), true);
                        break;
                    case ILOpCode.Ldsfld or ILOpCode.Ldsflda or ILOpCode.Stsfld:
                        Touched(MetadataTokens.EntityHandle(token));
                        break;
                }
            }
        }

        return new Walked(strings, calls, modules, links, unaddressed);
    }

    private const string AnchorEnd = "</a>";
}

internal static class Promised
{
    public const string Harness = "Alis.Reactive.Conformance.PlaywrightTests.Harness";

    private const string Mark = "Alis.Reactive.Conformance.World.FailsForAttribute";

    public static IEnumerable<(string Promise, MethodDefinitionHandle Method)> Tests(Loaded tests)
    {
        foreach (var typeHandle in tests.Reader.TypeDefinitions.Where(type => !Meta.NamespaceOf(tests.Reader, type).StartsWith(Harness, StringComparison.Ordinal)))
        {
            foreach (var methodHandle in tests.Reader.GetTypeDefinition(typeHandle).GetMethods())
            {
                foreach (var attributeHandle in tests.Reader.GetMethodDefinition(methodHandle).GetCustomAttributes())
                {
                    var attribute = tests.Reader.GetCustomAttribute(attributeHandle);
                    if (Meta.AttributeName(tests.Reader, attribute) != Mark) continue;
                    var value = tests.Reader.GetBlobReader(attribute.Value);
                    value.ReadUInt16();
                    yield return (value.ReadSerializedString() ?? "", methodHandle);
                }
            }
        }
    }
}

internal static class Meta
{
    public static string NameOf(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var type = reader.GetTypeDefinition(handle);
        var declaring = type.GetDeclaringType();
        return declaring.IsNil ? Joined(reader.GetString(type.Namespace), reader.GetString(type.Name)) : NameOf(reader, declaring) + "/" + reader.GetString(type.Name);
    }

    public static string NameOf(MetadataReader reader, TypeReferenceHandle handle)
    {
        var type = reader.GetTypeReference(handle);
        return type.ResolutionScope.Kind == HandleKind.TypeReference
            ? NameOf(reader, (TypeReferenceHandle)type.ResolutionScope) + "/" + reader.GetString(type.Name)
            : Joined(reader.GetString(type.Namespace), reader.GetString(type.Name));
    }

    public static string NamespaceOf(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var type = reader.GetTypeDefinition(handle);
        var declaring = type.GetDeclaringType();
        return declaring.IsNil ? reader.GetString(type.Namespace) : NamespaceOf(reader, declaring);
    }

    public static string ModuleOf(MetadataReader reader, TypeDefinitionHandle handle, string assembly)
    {
        var space = NamespaceOf(reader, handle);
        return space.StartsWith(assembly + ".", StringComparison.Ordinal) ? space[(assembly.Length + 1)..] : "";
    }

    public static (string Assembly, string Type, TypeDefinitionHandle? Own) ParentOf(MetadataReader reader, EntityHandle parent, string own)
    {
        switch (parent.Kind)
        {
            case HandleKind.TypeReference:
                return (AssemblyOf(reader, (TypeReferenceHandle)parent, own), NameOf(reader, (TypeReferenceHandle)parent), null);
            case HandleKind.TypeDefinition:
                return (own, NameOf(reader, (TypeDefinitionHandle)parent), (TypeDefinitionHandle)parent);
            case HandleKind.TypeSpecification:
                var blob = reader.GetBlobReader(reader.GetTypeSpecification((TypeSpecificationHandle)parent).Signature);
                if (blob.ReadSignatureTypeCode() != SignatureTypeCode.GenericTypeInstance) return ("", "", null);
                blob.ReadSignatureTypeCode();
                return ParentOf(reader, blob.ReadTypeHandle(), own);
            default:
                return ("", "", null);
        }
    }

    public static bool TheFrameworkCanCall(MetadataReader reader, MethodDefinitionHandle handle)
    {
        var method = reader.GetMethodDefinition(handle);
        return (method.Attributes & MethodAttributes.Virtual) != 0 || reader.GetString(method.Name) == ".ctor";
    }

    public static string AttributeName(MetadataReader reader, CustomAttribute attribute)
    {
        var parent = attribute.Constructor.Kind == HandleKind.MemberReference
            ? reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent
            : reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType();
        return parent.Kind switch
        {
            HandleKind.TypeReference => NameOf(reader, (TypeReferenceHandle)parent),
            HandleKind.TypeDefinition => NameOf(reader, (TypeDefinitionHandle)parent),
            _ => "",
        };
    }

    public static int ParameterCount(MetadataReader reader, CustomAttribute attribute)
    {
        var names = new Names(reader);
        return attribute.Constructor.Kind == HandleKind.MemberReference
            ? reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).DecodeMethodSignature(names, null).ParameterTypes.Length
            : reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).DecodeSignature(names, null).ParameterTypes.Length;
    }

    public static string Key(MethodSignature<string> signature)
        => $"`{signature.GenericParameterCount}({string.Join(",", signature.ParameterTypes)})->{signature.ReturnType}{(signature.Header.IsInstance ? "" : " static")}";

    private static string AssemblyOf(MetadataReader reader, TypeReferenceHandle handle, string own)
    {
        var scope = reader.GetTypeReference(handle).ResolutionScope;
        return scope.Kind switch
        {
            HandleKind.TypeReference => AssemblyOf(reader, (TypeReferenceHandle)scope, own),
            HandleKind.AssemblyReference => reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)scope).Name),
            _ => own,
        };
    }

    private static string Joined(string space, string name) => space.Length == 0 ? name : space + "." + name;
}

internal sealed class Names(MetadataReader reader, HashSet<TypeDefinitionHandle>? seen = null) : ISignatureTypeProvider<string, object?>
{
    public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[" + new string(',', shape.Rank - 1) + "]";

    public string GetByReferenceType(string elementType) => elementType + "&";

    public string GetFunctionPointerType(MethodSignature<string> signature) => "method" + Meta.Key(signature);

    public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) => genericType + "<" + string.Join(",", typeArguments) + ">";

    public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;

    public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;

    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => (isRequired ? "modreq(" : "modopt(") + modifier + ")" + unmodifiedType;

    public string GetPinnedType(string elementType) => elementType + " pinned";

    public string GetPointerType(string elementType) => elementType + "*";

    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();

    public string GetSZArrayType(string elementType) => elementType + "[]";

    public string GetTypeFromDefinition(MetadataReader metadata, TypeDefinitionHandle handle, byte rawTypeKind)
    {
        seen?.Add(handle);
        return Meta.NameOf(metadata, handle);
    }

    public string GetTypeFromReference(MetadataReader metadata, TypeReferenceHandle handle, byte rawTypeKind) => Meta.NameOf(metadata, handle);

    public string GetTypeFromSpecification(MetadataReader metadata, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
}

internal static class Il
{
    public static IEnumerable<(ILOpCode Code, int Token)> Read(byte[] il)
    {
        var at = 0;
        while (at < il.Length)
        {
            int value = il[at++];
            if (value == 0xFE) value = 0xFE00 | il[at++];
            var code = (ILOpCode)value;
            var size = OperandSize(code, il, at);
            yield return (code, size == 4 ? BitConverter.ToInt32(il, at) : 0);
            at += size;
        }
    }

    private static int OperandSize(ILOpCode code, byte[] il, int at) => code switch
    {
        ILOpCode.Switch => 4 + (4 * BitConverter.ToInt32(il, at)),
        ILOpCode.Ldc_i8 or ILOpCode.Ldc_r8 => 8,
        ILOpCode.Ldarg or ILOpCode.Ldarga or ILOpCode.Starg or ILOpCode.Ldloc or ILOpCode.Ldloca or ILOpCode.Stloc => 2,
        ILOpCode.Ldarg_s or ILOpCode.Ldarga_s or ILOpCode.Starg_s or ILOpCode.Ldloc_s or ILOpCode.Ldloca_s or ILOpCode.Stloc_s or ILOpCode.Ldc_i4_s or ILOpCode.Unaligned => 1,
        _ when code.IsBranch() => code.GetBranchOperandSize(),
        ILOpCode.Ldc_i4 or ILOpCode.Ldc_r4 or ILOpCode.Jmp or ILOpCode.Call or ILOpCode.Calli or ILOpCode.Callvirt or ILOpCode.Cpobj or ILOpCode.Ldobj
            or ILOpCode.Ldstr or ILOpCode.Newobj or ILOpCode.Castclass or ILOpCode.Isinst or ILOpCode.Unbox or ILOpCode.Ldfld or ILOpCode.Ldflda
            or ILOpCode.Stfld or ILOpCode.Ldsfld or ILOpCode.Ldsflda or ILOpCode.Stsfld or ILOpCode.Stobj or ILOpCode.Box or ILOpCode.Newarr
            or ILOpCode.Ldelema or ILOpCode.Ldelem or ILOpCode.Stelem or ILOpCode.Unbox_any or ILOpCode.Refanyval or ILOpCode.Mkrefany
            or ILOpCode.Ldtoken or ILOpCode.Ldftn or ILOpCode.Ldvirtftn or ILOpCode.Initobj or ILOpCode.Constrained or ILOpCode.Sizeof => 4,
        _ => 0,
    };
}
