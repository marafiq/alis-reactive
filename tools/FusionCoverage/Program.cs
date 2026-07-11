using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace FusionCoverage
{
    // Deterministic "no sandbox usage" coverage signal. An in-build Roslyn analyzer cannot see
    // Razor source-generated view trees, so this reads compiled metadata instead: the public
    // Fusion/Native component-slice surface (from their dlls) versus every member the SandboxApp
    // dll references (its MemberReference table — which includes view calls, since views compile
    // into the dll). A surface member with zero references is referenced nowhere in the sandbox
    // assembly and cannot be Playwright-covered.
    //
    // Necessary, not sufficient: "referenced in the sandbox assembly" (a view, controller, or
    // model) is weaker than "exercised by a passing Playwright test" — behavioral proof still
    // needs the test (resolved from the TRX). This signal only proves the negative: zero
    // references => definitely uncovered.
    //
    // Keys carry the decoded parameter-type list so overloads do not collapse (a used overload
    // must not mark its unused siblings covered). IHtmlContent render-contract methods are
    // excluded: the Razor runtime invokes them via interface dispatch, never as a direct
    // reference, so they are structurally unflaggable and would be noise.
    internal static class Program
    {
        private static readonly TypeNameProvider Provider = new TypeNameProvider();

        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("usage: FusionCoverage <bin-dir-with-built-dlls>");
                return 2;
            }

            var binDir = args[0];
            string Dll(string name) => Path.Combine(binDir, name);

            var sandbox = Dll("Alis.Reactive.SandboxApp.dll");
            if (!File.Exists(sandbox))
            {
                Console.Error.WriteLine($"not found: {sandbox} (build SandboxApp first)");
                return 2;
            }

            var surface = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var dll in new[] { Dll("Alis.Reactive.Fusion.dll"), Dll("Alis.Reactive.Native.dll") })
                if (File.Exists(dll)) CollectSurface(dll, surface);

            var used = new HashSet<string>(StringComparer.Ordinal);
            CollectUsed(sandbox, used);

            var uncovered = surface.Where(key => !used.Contains(key)).ToList();
            Console.WriteLine($"public component-slice members       : {surface.Count}");
            Console.WriteLine($"referenced in sandbox assembly       : {surface.Count - uncovered.Count}");
            Console.WriteLine($"NO sandbox reference (uncovered)     : {uncovered.Count}");
            Console.WriteLine();
            foreach (var key in uncovered) Console.WriteLine(key);
            return 0;
        }

        private static bool InSlice(string ns) =>
            ns.StartsWith("Alis.Reactive.Fusion.Components", StringComparison.Ordinal)
            || ns.StartsWith("Alis.Reactive.Native.Components", StringComparison.Ordinal);

        private static bool InSliceFullName(string fullName) =>
            fullName.StartsWith("Alis.Reactive.Fusion.Components.", StringComparison.Ordinal)
            || fullName.StartsWith("Alis.Reactive.Native.Components.", StringComparison.Ordinal);

        private static bool IsAccessorOrContract(string name) =>
            name.StartsWith("get_", StringComparison.Ordinal)
            || name.StartsWith("set_", StringComparison.Ordinal)
            || name.StartsWith("add_", StringComparison.Ordinal)
            || name.StartsWith("remove_", StringComparison.Ordinal)
            || name.StartsWith("op_", StringComparison.Ordinal)
            || name.StartsWith(".", StringComparison.Ordinal)
            // IHtmlContent render contract — invoked by the Razor runtime via interface dispatch,
            // never a direct reference; structurally unflaggable, so excluded as noise.
            || name == "WriteTo"
            || name == "ToHtmlString";

        private static void CollectSurface(string path, SortedSet<string> surface)
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            var md = pe.GetMetadataReader();

            foreach (var handle in md.TypeDefinitions)
            {
                var type = md.GetTypeDefinition(handle);
                var ns = md.GetString(type.Namespace);
                if (!InSlice(ns)) continue;
                if ((type.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.Public) continue;

                var typeName = md.GetString(type.Name);
                foreach (var methodHandle in type.GetMethods())
                {
                    var method = md.GetMethodDefinition(methodHandle);
                    if ((method.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public) continue;

                    var name = md.GetString(method.Name);
                    if (IsAccessorOrContract(name)) continue;

                    var signature = method.DecodeSignature(Provider, null);
                    surface.Add(Key($"{ns}.{typeName}", name, signature.ParameterTypes));
                }
            }
        }

        private static void CollectUsed(string path, HashSet<string> used)
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            var md = pe.GetMetadataReader();

            foreach (var handle in md.MemberReferences)
            {
                var member = md.GetMemberReference(handle);
                if (member.GetKind() != MemberReferenceKind.Method) continue;

                var parentName = ResolveParentTypeName(md, member.Parent);
                if (parentName == null || !InSliceFullName(parentName)) continue;

                var signature = member.DecodeMethodSignature(Provider, null);
                used.Add(Key(parentName, md.GetString(member.Name), signature.ParameterTypes));
            }
        }

        // Both sides build the key the same way — same provider, same param encoding — so they join
        // while distinguishing overloads.
        private static string Key(string typeFullName, string method, ImmutableArray<string> parameterTypes) =>
            $"{typeFullName}.{method}({string.Join(",", parameterTypes)})";

        private static string? ResolveParentTypeName(MetadataReader md, EntityHandle parent)
        {
            switch (parent.Kind)
            {
                case HandleKind.TypeReference:
                    var tr = md.GetTypeReference((TypeReferenceHandle)parent);
                    return Combine(md.GetString(tr.Namespace), md.GetString(tr.Name));
                case HandleKind.TypeDefinition:
                    var td = md.GetTypeDefinition((TypeDefinitionHandle)parent);
                    return Combine(md.GetString(td.Namespace), md.GetString(td.Name));
                case HandleKind.TypeSpecification:
                    return md.GetTypeSpecification((TypeSpecificationHandle)parent).DecodeSignature(Provider, null);
                default:
                    return null;
            }
        }

        private static string Combine(string ns, string name) => ns.Length == 0 ? name : ns + "." + name;

        // Resolves a signature element to a type's full name; a generic instantiation collapses to
        // the underlying generic type name (consistent on both sides, so the join still holds while
        // distinguishing overloads by their other parameter types).
        private sealed class TypeNameProvider : ISignatureTypeProvider<string, object?>
        {
            public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
            {
                var td = reader.GetTypeDefinition(handle);
                return Combine(reader.GetString(td.Namespace), reader.GetString(td.Name));
            }

            public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
            {
                var tr = reader.GetTypeReference(handle);
                return Combine(reader.GetString(tr.Namespace), reader.GetString(tr.Name));
            }

            public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
                => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

            public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) => genericType;
            public string GetSZArrayType(string elementType) => elementType + "[]";
            public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[]";
            public string GetByReferenceType(string elementType) => elementType + "&";
            public string GetPointerType(string elementType) => elementType + "*";
            public string GetPinnedType(string elementType) => elementType;
            public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
            public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();
            public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;
            public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;
            public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
        }
    }
}
