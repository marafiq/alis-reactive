using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alis.Reactive.PlanContractGenerator;

internal static class PathSteps
{
    private const string Schema = "Alis.Reactive.Schema.";
    private const BindingFlags Internal = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

    internal static IReadOnlyList<WrittenType> Rows(Assembly core, JsonSerializerOptions options)
    {
        var pathStep = core.GetType(Schema + "PathStep", throwOnError: true)!;
        var registrationStep = core.GetType(Schema + "RegistrationStep", throwOnError: true)!;
        var each = core.GetType(Schema + "EachRegistrationStep", throwOnError: true)!;
        var member = Activator.CreateInstance(core.GetType(Schema + "MemberStep", throwOnError: true)!, Internal, binder: null, ["ward"], culture: null);
        var index = Activator.CreateInstance(core.GetType(Schema + "IndexStep", throwOnError: true)!, Internal, binder: null, [2], culture: null);

        Expect(JsonSerializer.Serialize(member, pathStep, options), "\"ward\"", "a member step");
        Expect(JsonSerializer.Serialize(index, pathStep, options), "2", "an index step");
        Expect(JsonSerializer.Serialize(each.GetField("Instance", Internal)!.GetValue(null), registrationStep, options), "{\"kind\":\"each\"}", "the step of every item");

        return
        [
            new WrittenType(
                pathStep,
                "export type PathStep = string | number;",
                new JsonObject
                {
                    ["oneOf"] = new JsonArray(new JsonObject { ["type"] = "string" }, new JsonObject { ["type"] = "integer", ["minimum"] = 0 }),
                },
                kinds: [],
                uses: []),
            new WrittenType(
                each,
                "export interface EachRegistrationStep {\n  readonly kind: \"each\";\n}",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject { ["kind"] = new JsonObject { ["const"] = "each" } },
                    ["required"] = new JsonArray("kind"),
                    ["additionalProperties"] = false,
                },
                kinds: ["each"],
                uses: []),
            new WrittenType(
                registrationStep,
                "export type RegistrationStep = PathStep | EachRegistrationStep;",
                new JsonObject
                {
                    ["oneOf"] = new JsonArray(new JsonObject { ["$ref"] = "#/$defs/PathStep" }, new JsonObject { ["$ref"] = "#/$defs/EachRegistrationStep" }),
                },
                kinds: [],
                uses: [pathStep, each]),
        ];
    }

    private static void Expect(string written, string expected, string step)
    {
        if (written != expected)
            throw new GrammarRefusedException($"The grammar writes {step} as {written}; the contract's row for it reads {expected}.");
    }
}
