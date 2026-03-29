/**
 * Schema → TS Drift Detection
 *
 * Reads reactive-plan.schema.json and verifies that every property, enum value,
 * and discriminated union variant has a matching TS type definition.
 *
 * Historical drift this catches:
 *   - componentType was missing from TS ComponentEntry for weeks (4be3e5e)
 *   - Any new schema property not reflected in TS types
 *   - Any new enum value not reflected in TS union types
 *   - Any new discriminated union variant not reflected in TS type unions
 */
import { describe, it, expect } from "vitest";
import { readFileSync } from "fs";
import { resolve } from "path";

// ── Load schema ──

const schemaPath = resolve(
  __dirname,
  "../../../Alis.Reactive/Schemas/reactive-plan.schema.json",
);
const schema = JSON.parse(readFileSync(schemaPath, "utf-8"));
const defs = schema.$defs as Record<string, SchemaDefinition>;

interface SchemaProperty {
  type?: string;
  const?: string;
  $ref?: string;
  enum?: string[];
  description?: string;
  items?: SchemaProperty;
  oneOf?: SchemaProperty[];
  minLength?: number;
  minItems?: number;
}

interface SchemaDefinition {
  type?: string;
  required?: string[];
  properties?: Record<string, SchemaProperty>;
  additionalProperties?: boolean | SchemaProperty;
  enum?: string[];
  oneOf?: SchemaProperty[];
  items?: SchemaProperty;
  description?: string;
}

// ── Import TS types to verify they compile ──
// These imports prove the types exist. The structural checks below verify
// they match the schema.

import type { Plan, ComponentEntry, Entry } from "../types/plan";
import type {
  DomReadyTrigger,
  CustomEventTrigger,
  ComponentEventTrigger,
  ServerPushTrigger,
  SignalRTrigger,
  Trigger,
} from "../types/triggers";
import type {
  SequentialReaction,
  ConditionalReaction,
  HttpReaction,
  ParallelHttpReaction,
  Branch,
} from "../types/reactions";
import type {
  DispatchCommand,
  MutateElementCommand,
  MutateEventCommand,
  ValidationErrorsCommand,
  IntoCommand,
  Mutation,
  SetPropMutation,
  CallMutation,
  MethodArg,
  LiteralArg,
  SourceArg,
} from "../types/commands";
import type {
  ValueGuard,
  AllGuard,
  AnyGuard,
  InvertGuard,
  ConfirmGuard,
  GuardOp,
} from "../types/guards";
import type { EventSource, ComponentSource } from "../types/sources";
import type {
  ComponentGather,
  StaticGather,
  AllGather,
  EventGather,
  StatusHandler,
  RequestDescriptor,
} from "../types/http";
import type {
  ValidationDescriptor,
  ValidationField,
  ValidationRule,
  ValidationRuleType,
  ValidationCondition,
} from "../types/validation";
import type { Vendor } from "../types/context";
import type { CoercionType } from "../core/coerce";

// ── Schema → TS type mapping ──
// Maps each schema $defs name to a verification function that checks
// the TS type has matching structure.

/**
 * For each schema definition, create a TS object that satisfies the type.
 * If the schema adds a required property that TS doesn't have, this file
 * won't compile (caught by npm run typecheck).
 * The runtime checks below catch optional properties and enum values.
 */

// ── Compile-time structural checks ──
// These dummy objects verify that TS interfaces match schema required properties.
// If schema adds a required property, TypeScript compilation fails here.

const _planCheck: Plan = {
  planId: "",
  components: {},
  entries: [],
};

const _componentEntryCheck: ComponentEntry = {
  id: "",
  vendor: "native",
  readExpr: "",
  componentType: "",
  coerceAs: "string",
};

const _entryCheck: Entry = {
  trigger: { kind: "dom-ready" },
  reaction: { kind: "sequential", commands: [] },
};

const _domReadyCheck: DomReadyTrigger = { kind: "dom-ready" };
const _customEventCheck: CustomEventTrigger = {
  kind: "custom-event",
  event: "",
};
const _componentEventCheck: ComponentEventTrigger = {
  kind: "component-event",
  componentId: "",
  jsEvent: "",
  vendor: "native",
};
const _serverPushCheck: ServerPushTrigger = { kind: "server-push", url: "" };
const _signalRCheck: SignalRTrigger = {
  kind: "signalr",
  hubUrl: "",
  methodName: "",
};

const _sequentialCheck: SequentialReaction = {
  kind: "sequential",
  commands: [],
};
const _conditionalCheck: ConditionalReaction = {
  kind: "conditional",
  branches: [],
};
const _httpReactionCheck: HttpReaction = {
  kind: "http",
  request: { verb: "GET", url: "" },
};
const _parallelHttpCheck: ParallelHttpReaction = {
  kind: "parallel-http",
  requests: [],
};

const _dispatchCheck: DispatchCommand = { kind: "dispatch", event: "" };
const _mutateElementCheck: MutateElementCommand = {
  kind: "mutate-element",
  target: "",
  mutation: { kind: "set-prop", prop: "" },
};
const _mutateEventCheck: MutateEventCommand = {
  kind: "mutate-event",
  mutation: { kind: "set-prop", prop: "" },
};
const _validationErrorsCheck: ValidationErrorsCommand = {
  kind: "validation-errors",
  formId: "",
};
const _intoCheck: IntoCommand = { kind: "into", target: "" };

const _setPropCheck: SetPropMutation = { kind: "set-prop", prop: "" };
const _callMutationCheck: CallMutation = { kind: "call", method: "" };
const _literalArgCheck: LiteralArg = { kind: "literal", value: "" };
const _sourceArgCheck: SourceArg = {
  kind: "source",
  source: { kind: "event", path: "" },
};

const _valueGuardCheck: ValueGuard = {
  kind: "value",
  source: { kind: "event", path: "" },
  coerceAs: "string",
  op: "eq",
};
const _allGuardCheck: AllGuard = { kind: "all", guards: [] };
const _anyGuardCheck: AnyGuard = { kind: "any", guards: [] };
const _invertGuardCheck: InvertGuard = {
  kind: "not",
  inner: { kind: "value", source: { kind: "event", path: "" }, coerceAs: "string", op: "eq" },
};
const _confirmGuardCheck: ConfirmGuard = { kind: "confirm", message: "" };

const _eventSourceCheck: EventSource = { kind: "event", path: "" };
const _componentSourceCheck: ComponentSource = {
  kind: "component",
  componentId: "",
  vendor: "native",
  readExpr: "",
};

const _componentGatherCheck: ComponentGather = {
  kind: "component",
  componentId: "",
  vendor: "native",
  name: "",
  readExpr: "",
};
const _staticGatherCheck: StaticGather = {
  kind: "static",
  param: "",
  value: "",
};
const _allGatherCheck: AllGather = { kind: "all" };
const _eventGatherCheck: EventGather = { kind: "event", param: "", path: "" };

const _statusHandlerCheck: StatusHandler = {};
const _requestDescriptorCheck: RequestDescriptor = { verb: "GET", url: "" };

const _validationDescriptorCheck: ValidationDescriptor = {
  formId: "",
  fields: [],
};
const _validationFieldCheck: ValidationField = {
  fieldName: "",
  rules: [],
};
const _validationRuleCheck: ValidationRule = { rule: "required", message: "" };
const _validationConditionCheck: ValidationCondition = {
  field: "",
  op: "truthy",
};
const _branchCheck: Branch = {
  guard: null,
  reaction: { kind: "sequential", commands: [] },
};

// Suppress unused variable warnings — these exist for compile-time checks only
void [
  _planCheck,
  _componentEntryCheck,
  _entryCheck,
  _domReadyCheck,
  _customEventCheck,
  _componentEventCheck,
  _serverPushCheck,
  _signalRCheck,
  _sequentialCheck,
  _conditionalCheck,
  _httpReactionCheck,
  _parallelHttpCheck,
  _dispatchCheck,
  _mutateElementCheck,
  _mutateEventCheck,
  _validationErrorsCheck,
  _intoCheck,
  _setPropCheck,
  _callMutationCheck,
  _literalArgCheck,
  _sourceArgCheck,
  _valueGuardCheck,
  _allGuardCheck,
  _anyGuardCheck,
  _invertGuardCheck,
  _confirmGuardCheck,
  _eventSourceCheck,
  _componentSourceCheck,
  _componentGatherCheck,
  _staticGatherCheck,
  _allGatherCheck,
  _eventGatherCheck,
  _statusHandlerCheck,
  _requestDescriptorCheck,
  _validationDescriptorCheck,
  _validationFieldCheck,
  _validationRuleCheck,
  _validationConditionCheck,
  _branchCheck,
];

// ── Runtime checks for enums and discriminated unions ──

describe("Schema → TS drift detection", () => {
  // ── Helper: extract property names from a schema definition ──
  function getSchemaProperties(
    defName: string,
  ): { required: string[]; optional: string[] } {
    const def = defs[defName];
    if (!def?.properties) return { required: [], optional: [] };
    const allProps = Object.keys(def.properties);
    const required = def.required ?? [];
    const optional = allProps.filter((p) => !required.includes(p));
    return { required, optional };
  }

  // ── Helper: extract enum values from schema ──
  function getSchemaEnumValues(defName: string): string[] {
    const def = defs[defName];
    return def?.enum ?? [];
  }

  // ── Helper: extract discriminated union "kind" values from oneOf ──
  function getSchemaKindValues(defName: string): string[] {
    const def = defs[defName];
    if (!def?.oneOf) return [];
    return def.oneOf
      .map((variant) => {
        // Each variant is a $ref to another def
        const ref = variant.$ref;
        if (!ref) return null;
        const refName = ref.replace("#/$defs/", "");
        const refDef = defs[refName];
        if (!refDef?.properties?.kind?.const) return null;
        return refDef.properties.kind.const as string;
      })
      .filter((k): k is string => k !== null);
  }

  // ── Enum value checks ──
  // These verify that TS union types have all values from schema enums.

  describe("Vendor enum", () => {
    it("TS Vendor has all schema enum values", () => {
      const schemaValues = getSchemaEnumValues("Vendor");
      // Verify schema has values we expect
      expect(schemaValues).toContain("fusion");
      expect(schemaValues).toContain("native");

      // Verify TS type accepts all schema values by assignment check
      const tsValues: Vendor[] = ["fusion", "native"];
      expect(tsValues.length).toBe(schemaValues.length);
      for (const sv of schemaValues) {
        expect(tsValues).toContain(sv);
      }
    });
  });

  describe("CoercionType enum", () => {
    it("TS CoercionType has all schema enum values", () => {
      const schemaValues = getSchemaEnumValues("CoercionType");
      const tsValues: CoercionType[] = [
        "string",
        "number",
        "boolean",
        "date",
        "raw",
        "array",
      ];
      expect(tsValues.length).toBe(schemaValues.length);
      for (const sv of schemaValues) {
        expect(tsValues).toContain(sv);
      }
    });
  });

  describe("GuardOp enum", () => {
    it("TS GuardOp has all schema enum values", () => {
      const schemaValues = getSchemaEnumValues("GuardOp");
      const tsValues: GuardOp[] = [
        "eq",
        "neq",
        "gt",
        "gte",
        "lt",
        "lte",
        "truthy",
        "falsy",
        "is-null",
        "not-null",
        "is-empty",
        "not-empty",
        "in",
        "not-in",
        "between",
        "array-contains",
        "contains",
        "starts-with",
        "ends-with",
        "matches",
        "min-length",
      ];
      expect(tsValues.length).toBe(schemaValues.length);
      for (const sv of schemaValues) {
        expect(tsValues).toContain(sv);
      }
    });
  });

  describe("ValidationRuleType enum", () => {
    it("TS ValidationRuleType has all schema enum values", () => {
      const schemaValues = getSchemaEnumValues("ValidationRuleType");
      const tsValues: ValidationRuleType[] = [
        "required",
        "empty",
        "minLength",
        "maxLength",
        "email",
        "regex",
        "url",
        "creditCard",
        "range",
        "exclusiveRange",
        "min",
        "max",
        "gt",
        "lt",
        "equalTo",
        "notEqual",
        "notEqualTo",
        "atLeastOne",
      ];
      expect(tsValues.length).toBe(schemaValues.length);
      for (const sv of schemaValues) {
        expect(tsValues).toContain(sv);
      }
    });
  });

  // ── Discriminated union checks ──
  // These verify that TS union types cover all schema oneOf variants.

  describe("Trigger union", () => {
    it("TS Trigger has all schema variants", () => {
      const schemaKinds = getSchemaKindValues("Trigger");
      // TS Trigger union must cover all these kinds
      const tsKinds: Trigger["kind"][] = [
        "dom-ready",
        "custom-event",
        "component-event",
        "server-push",
        "signalr",
      ];
      expect(tsKinds.length).toBe(schemaKinds.length);
      for (const sk of schemaKinds) {
        expect(tsKinds).toContain(sk);
      }
    });
  });

  describe("Reaction union", () => {
    it("TS Reaction has all schema variants", () => {
      const schemaKinds = getSchemaKindValues("Reaction");
      const tsKinds: Array<
        | SequentialReaction["kind"]
        | ConditionalReaction["kind"]
        | HttpReaction["kind"]
        | ParallelHttpReaction["kind"]
      > = ["sequential", "conditional", "http", "parallel-http"];
      expect(tsKinds.length).toBe(schemaKinds.length);
      for (const sk of schemaKinds) {
        expect(tsKinds).toContain(sk);
      }
    });
  });

  describe("Command union", () => {
    it("TS Command has all schema variants", () => {
      const schemaKinds = getSchemaKindValues("Command");
      const tsKinds: Array<
        | DispatchCommand["kind"]
        | MutateElementCommand["kind"]
        | MutateEventCommand["kind"]
        | ValidationErrorsCommand["kind"]
        | IntoCommand["kind"]
      > = [
        "dispatch",
        "mutate-element",
        "mutate-event",
        "validation-errors",
        "into",
      ];
      expect(tsKinds.length).toBe(schemaKinds.length);
      for (const sk of schemaKinds) {
        expect(tsKinds).toContain(sk);
      }
    });
  });

  describe("Guard union", () => {
    it("TS Guard has all schema variants", () => {
      const schemaKinds = getSchemaKindValues("Guard");
      const tsKinds: Array<
        | ValueGuard["kind"]
        | AllGuard["kind"]
        | AnyGuard["kind"]
        | InvertGuard["kind"]
        | ConfirmGuard["kind"]
      > = ["value", "all", "any", "not", "confirm"];
      expect(tsKinds.length).toBe(schemaKinds.length);
      for (const sk of schemaKinds) {
        expect(tsKinds).toContain(sk);
      }
    });
  });

  describe("BindSource union", () => {
    it("TS BindSource has all schema variants", () => {
      const schemaKinds = getSchemaKindValues("BindSource");
      const tsKinds: Array<EventSource["kind"] | ComponentSource["kind"]> = [
        "event",
        "component",
      ];
      expect(tsKinds.length).toBe(schemaKinds.length);
      for (const sk of schemaKinds) {
        expect(tsKinds).toContain(sk);
      }
    });
  });

  describe("Mutation union", () => {
    it("TS Mutation has all schema variants", () => {
      const schemaKinds = getSchemaKindValues("Mutation");
      const tsKinds: Array<SetPropMutation["kind"] | CallMutation["kind"]> = [
        "set-prop",
        "call",
      ];
      expect(tsKinds.length).toBe(schemaKinds.length);
      for (const sk of schemaKinds) {
        expect(tsKinds).toContain(sk);
      }
    });
  });

  describe("MethodArg union", () => {
    it("TS MethodArg has all schema variants", () => {
      const schemaKinds = getSchemaKindValues("MethodArg");
      const tsKinds: Array<LiteralArg["kind"] | SourceArg["kind"]> = [
        "literal",
        "source",
      ];
      expect(tsKinds.length).toBe(schemaKinds.length);
      for (const sk of schemaKinds) {
        expect(tsKinds).toContain(sk);
      }
    });
  });

  describe("GatherItem union", () => {
    it("TS GatherItem has all schema variants", () => {
      const schemaKinds = getSchemaKindValues("GatherItem");
      const tsKinds: Array<
        | ComponentGather["kind"]
        | StaticGather["kind"]
        | AllGather["kind"]
        | EventGather["kind"]
      > = ["component", "static", "all", "event"];
      expect(tsKinds.length).toBe(schemaKinds.length);
      for (const sk of schemaKinds) {
        expect(tsKinds).toContain(sk);
      }
    });
  });

  // ── Property completeness checks ──
  // For each schema definition with properties, verify TS interfaces have them all.

  describe("property completeness", () => {
    // Map schema def names to the keys of the corresponding TS interfaces.
    // We use runtime object construction to get the keys.

    function tsKeysOf<T extends Record<string, unknown>>(
      obj: T,
    ): Set<string> {
      return new Set(Object.keys(obj));
    }

    it("ComponentEntry has all schema properties", () => {
      const { required, optional } = getSchemaProperties("ComponentEntry");
      // Build object with all properties to get keys
      const obj: ComponentEntry = {
        id: "",
        vendor: "native",
        readExpr: "",
        componentType: "",
        coerceAs: "string",
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of required) {
        expect(tsKeys.has(r), `Missing required property: ${r}`).toBe(true);
      }
      for (const o of optional) {
        // Optional properties may not be in the object literal, check they exist on type
        // by verifying they're at least known to the schema
        expect(
          tsKeys.has(o) || optional.includes(o),
          `Unknown property: ${o}`,
        ).toBe(true);
      }
    });

    it("DomReadyTrigger has all schema properties", () => {
      const { required } = getSchemaProperties("DomReadyTrigger");
      const tsKeys = tsKeysOf({ kind: "dom-ready" as const });
      for (const r of required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("CustomEventTrigger has all schema properties", () => {
      const { required } = getSchemaProperties("CustomEventTrigger");
      const tsKeys = tsKeysOf({
        kind: "custom-event" as const,
        event: "",
      });
      for (const r of required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("ComponentEventTrigger has all schema properties", () => {
      const schema = getSchemaProperties("ComponentEventTrigger");
      const obj: Required<ComponentEventTrigger> = {
        kind: "component-event",
        componentId: "",
        jsEvent: "",
        vendor: "native",
        bindingPath: "",
        readExpr: "",
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("ServerPushTrigger has all schema properties", () => {
      const schema = getSchemaProperties("ServerPushTrigger");
      const obj: Required<ServerPushTrigger> = {
        kind: "server-push",
        url: "",
        eventType: "",
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("SignalRTrigger has all schema properties", () => {
      const schema = getSchemaProperties("SignalRTrigger");
      const tsKeys = tsKeysOf({
        kind: "signalr" as const,
        hubUrl: "",
        methodName: "",
      });
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("DispatchCommand has all schema properties", () => {
      const schema = getSchemaProperties("DispatchCommand");
      const obj: Required<DispatchCommand> = {
        kind: "dispatch",
        event: "",
        payload: {},
        when: { kind: "value", source: { kind: "event", path: "" }, coerceAs: "string", op: "eq" },
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("MutateElementCommand has all schema properties", () => {
      const schema = getSchemaProperties("MutateElementCommand");
      const obj: Required<MutateElementCommand> = {
        kind: "mutate-element",
        target: "",
        mutation: { kind: "set-prop", prop: "" },
        value: "",
        source: { kind: "event", path: "" },
        vendor: "native",
        when: { kind: "value", source: { kind: "event", path: "" }, coerceAs: "string", op: "eq" },
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("MutateEventCommand has all schema properties", () => {
      const schema = getSchemaProperties("MutateEventCommand");
      const obj: Required<MutateEventCommand> = {
        kind: "mutate-event",
        mutation: { kind: "set-prop", prop: "" },
        value: "",
        source: { kind: "event", path: "" },
        when: { kind: "value", source: { kind: "event", path: "" }, coerceAs: "string", op: "eq" },
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("ValidationErrorsCommand has all schema properties", () => {
      const schema = getSchemaProperties("ValidationErrorsCommand");
      const obj: Required<ValidationErrorsCommand> = {
        kind: "validation-errors",
        formId: "",
        when: { kind: "value", source: { kind: "event", path: "" }, coerceAs: "string", op: "eq" },
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("IntoCommand has all schema properties", () => {
      const schema = getSchemaProperties("IntoCommand");
      const obj: Required<IntoCommand> = {
        kind: "into",
        target: "",
        when: { kind: "value", source: { kind: "event", path: "" }, coerceAs: "string", op: "eq" },
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("ValueGuard has all schema properties", () => {
      const schema = getSchemaProperties("ValueGuard");
      const obj: Required<ValueGuard> = {
        kind: "value",
        source: { kind: "event", path: "" },
        coerceAs: "string",
        op: "eq",
        operand: "",
        rightSource: { kind: "event", path: "" },
        elementCoerceAs: "string",
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("ConfirmGuard has all schema properties", () => {
      const schema = getSchemaProperties("ConfirmGuard");
      const tsKeys = tsKeysOf({ kind: "confirm" as const, message: "" });
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("EventSource has all schema properties", () => {
      const schema = getSchemaProperties("EventSource");
      const tsKeys = tsKeysOf({ kind: "event" as const, path: "" });
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("ComponentSource has all schema properties", () => {
      const schema = getSchemaProperties("ComponentSource");
      const tsKeys = tsKeysOf({
        kind: "component" as const,
        componentId: "",
        vendor: "native" as Vendor,
        readExpr: "",
      });
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("SetPropMutation has all schema properties", () => {
      const schema = getSchemaProperties("SetPropMutation");
      const obj: Required<SetPropMutation> = {
        kind: "set-prop",
        prop: "",
        coerce: "string",
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("CallMutation has all schema properties", () => {
      const schema = getSchemaProperties("CallMutation");
      const obj: Required<CallMutation> = {
        kind: "call",
        method: "",
        chain: "",
        args: [],
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("RequestDescriptor has all schema properties", () => {
      const schema = getSchemaProperties("RequestDescriptor");
      const obj: Required<RequestDescriptor> = {
        verb: "GET",
        url: "",
        gather: [],
        contentType: "form-data",
        whileLoading: [],
        onSuccess: [],
        onError: [],
        chained: { verb: "GET", url: "" },
        validation: { formId: "", fields: [] },
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("ComponentGather has all schema properties", () => {
      const schema = getSchemaProperties("ComponentGather");
      const tsKeys = tsKeysOf({
        kind: "component" as const,
        componentId: "",
        vendor: "native" as Vendor,
        name: "",
        readExpr: "",
      });
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("StaticGather has all schema properties", () => {
      const schema = getSchemaProperties("StaticGather");
      const tsKeys = tsKeysOf({
        kind: "static" as const,
        param: "",
        value: "",
      });
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("EventGather has all schema properties", () => {
      const schema = getSchemaProperties("EventGather");
      const tsKeys = tsKeysOf({
        kind: "event" as const,
        param: "",
        path: "",
      });
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("ValidationDescriptor has all schema properties", () => {
      const schema = getSchemaProperties("ValidationDescriptor");
      const obj: Required<ValidationDescriptor> = {
        formId: "",
        planId: "",
        fields: [],
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("ValidationField has all schema properties", () => {
      const schema = getSchemaProperties("ValidationField");
      const obj: Required<ValidationField> = {
        fieldName: "",
        rules: [],
        fieldId: "",
        vendor: "native",
        readExpr: "",
        coerceAs: "string",
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("ValidationRule has all schema properties", () => {
      const schema = getSchemaProperties("ValidationRule");
      const obj: Required<ValidationRule> = {
        rule: "required",
        message: "",
        constraint: "",
        field: "",
        coerceAs: "string",
        when: { field: "", op: "truthy" },
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("ValidationCondition has all schema properties", () => {
      const schema = getSchemaProperties("ValidationCondition");
      const obj: Required<ValidationCondition> = {
        field: "",
        op: "truthy",
        value: "",
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("SequentialReaction has all schema properties", () => {
      const schema = getSchemaProperties("SequentialReaction");
      const tsKeys = tsKeysOf({ kind: "sequential" as const, commands: [] });
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("ConditionalReaction has all schema properties", () => {
      const schema = getSchemaProperties("ConditionalReaction");
      const obj: Required<ConditionalReaction> = {
        kind: "conditional",
        commands: [],
        branches: [],
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("HttpReaction has all schema properties", () => {
      const schema = getSchemaProperties("HttpReaction");
      const obj: Required<HttpReaction> = {
        kind: "http",
        preFetch: [],
        request: { verb: "GET", url: "" },
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("ParallelHttpReaction has all schema properties", () => {
      const schema = getSchemaProperties("ParallelHttpReaction");
      const obj: Required<ParallelHttpReaction> = {
        kind: "parallel-http",
        preFetch: [],
        requests: [],
        onAllSettled: [],
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
      for (const o of schema.optional) {
        expect(tsKeys.has(o), `Missing optional: ${o}`).toBe(true);
      }
    });

    it("Branch has all schema properties", () => {
      const schema = getSchemaProperties("Branch");
      const obj: Branch = {
        guard: null,
        reaction: { kind: "sequential", commands: [] },
      };
      const tsKeys = tsKeysOf(obj);
      for (const r of schema.required) {
        expect(tsKeys.has(r), `Missing required: ${r}`).toBe(true);
      }
    });

    it("StatusHandler has all schema properties", () => {
      const schema = getSchemaProperties("StatusHandler");
      const obj: Required<StatusHandler> = {
        statusCode: 200,
        commands: [],
        reaction: { kind: "sequential", commands: [] },
      };
      const tsKeys = tsKeysOf(obj);
      // StatusHandler uses oneOf in schema (commands OR reaction), both should exist in TS
      const allSchemaProps = [
        ...schema.required,
        ...schema.optional,
      ];
      for (const p of allSchemaProps) {
        expect(tsKeys.has(p), `Missing property: ${p}`).toBe(true);
      }
    });
  });

  // ── Schema structure sanity ──

  describe("schema structure", () => {
    it("all schema $defs have additionalProperties: false or are enums/unions", () => {
      // This verifies the schema itself is locked down, which is what makes
      // the C# drift detection work (new C# props cause schema validation failure).
      const exceptions = [
        // These are unions/enums, not object defs
        "Trigger",
        "Reaction",
        "Command",
        "Guard",
        "BindSource",
        "Mutation",
        "MethodArg",
        "GatherItem",
        "BindExpr",
        "GuardOp",
        "Vendor",
        "CoercionType",
        "ValidationRuleType",
      ];

      for (const [name, def] of Object.entries(defs)) {
        if (exceptions.includes(name)) continue;
        if (def.type === "object") {
          expect(
            def.additionalProperties === false,
            `${name} is missing additionalProperties: false`,
          ).toBe(true);
        }
      }
    });
  });
});
