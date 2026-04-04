import type {
  ActionTarget,
  BindingMapValueExpr,
  BranchCase,
  CapabilityContract,
  CompareOp,
  ContractKind,
  ContractResolver,
  EventContract,
  EventObjectReference,
  FieldBinding,
  PathSegment,
  Plan,
  PlanAction,
  PlanPredicate,
  PlanSubscription,
  RequestInput,
  RequestInputValue,
  RequestPlan,
  RequestValidation,
  RequestValidationField,
  RequestValidationRule,
  ResponseHandlerPlan,
  RuntimeObject,
  ValidationRuleName,
  ValueExpr,
  ValueShape,
  Workflow,
} from "../types";

export interface NativeActionLinkPayload {
  plan: Plan;
  action: PlanAction;
}

export function decodePlanDocument(value: unknown): Plan {
  const plan = expectRecord(value, "plan");
  expectExactNumber(plan.version, 2, "plan.version");

  return {
    version: 2,
    planId: expectString(plan.planId, "plan.planId"),
    ...(plan.sourceId === undefined ? {} : { sourceId: expectOptionalString(plan.sourceId, "plan.sourceId") }),
    contracts: decodeMap(plan.contracts, "plan.contracts", decodeContract),
    objects: decodeMap(plan.objects, "plan.objects", decodeObject),
    bindings: decodeMap(plan.bindings, "plan.bindings", decodeBinding),
    workflows: decodeArray(plan.workflows, "plan.workflows", decodeWorkflow),
  };
}

export function decodeNativeActionLinkPayload(value: unknown): NativeActionLinkPayload {
  const payload = expectRecord(value, "nativeActionLink");
  return {
    plan: decodePlanDocument(payload.plan),
    action: decodeAction(payload.action, "nativeActionLink.action"),
  };
}

function decodeContract(value: unknown, path: string): CapabilityContract {
  const contract = expectRecord(value, path);
  return {
    kind: expectOneOf(contract.kind, ["component", "element", "event-object", "service"], `${path}.kind`),
    resolver: expectOneOf(
      contract.resolver,
      ["native-element", "fusion-instance", "event-object", "context-object"],
      `${path}.resolver`
    ),
    members: decodeMap(contract.members, `${path}.members`, decodeMember),
    ...(contract.events === undefined
      ? {}
      : { events: decodeMap(contract.events, `${path}.events`, decodeEventContract) }),
  };
}

function decodeMember(value: unknown, path: string): CapabilityContract["members"][string] {
  const member = expectRecord(value, path);
  const kind = expectOneOf(member.kind, ["property", "method"], `${path}.kind`);

  if (kind === "property") {
    return {
      kind,
      path: decodeArray(member.path, `${path}.path`, decodePathSegment),
      shape: decodeShape(member.shape, `${path}.shape`),
      access: expectOneOf(member.access, ["read", "write", "readwrite"], `${path}.access`),
    };
  }

  return {
    kind,
    path: decodeArray(member.path, `${path}.path`, decodePathSegment),
    ...(member.args === undefined
      ? {}
      : { args: decodeArray(member.args, `${path}.args`, decodeShape) }),
    ...(member.returns === undefined
      ? {}
      : { returns: member.returns === "void" ? "void" : decodeShape(member.returns, `${path}.returns`) }),
  };
}

function decodeEventContract(value: unknown, path: string): EventContract {
  const event = expectRecord(value, path);
  return {
    channel: expectString(event.channel, `${path}.channel`),
    ...(event.eventObject === undefined
      ? {}
      : { eventObject: decodeEventObjectReference(event.eventObject, `${path}.eventObject`) }),
    ...(event.data === undefined
      ? {}
      : { data: decodeMap(event.data, `${path}.data`, decodeValueExpr) }),
  };
}

function decodeEventObjectReference(value: unknown, path: string): EventObjectReference {
  const reference = expectRecord(value, path);
  return {
    contract: expectString(reference.contract, `${path}.contract`),
  };
}

function decodeObject(value: unknown, path: string): RuntimeObject {
  const objectRef = expectRecord(value, path);
  return {
    contract: expectString(objectRef.contract, `${path}.contract`),
    ...(objectRef.elementId === undefined ? {} : { elementId: expectOptionalString(objectRef.elementId, `${path}.elementId`) }),
  };
}

function decodeBinding(value: unknown, path: string): FieldBinding {
  const binding = expectRecord(value, path);
  return {
    object: expectString(binding.object, `${path}.object`),
    valueMember: expectString(binding.valueMember, `${path}.valueMember`),
    shape: decodeShape(binding.shape, `${path}.shape`),
  };
}

function decodeWorkflow(value: unknown, path: string): Workflow {
  const workflow = expectRecord(value, path);
  return {
    when: decodeSubscription(workflow.when, `${path}.when`),
    run: decodeAction(workflow.run, `${path}.run`),
  };
}

function decodeSubscription(value: unknown, path: string): PlanSubscription {
  const subscription = expectRecord(value, path);
  const kind = expectOneOf(
    subscription.kind,
    ["dom-ready", "document-event", "object-event", "server-push", "signalr"],
    `${path}.kind`
  );

  switch (kind) {
    case "dom-ready":
      return { kind };
    case "document-event":
      return {
        kind,
        name: expectString(subscription.name, `${path}.name`),
      };
    case "object-event":
      return {
        kind,
        object: expectString(subscription.object, `${path}.object`),
        event: expectString(subscription.event, `${path}.event`),
      };
    case "server-push":
      return {
        kind,
        url: expectString(subscription.url, `${path}.url`),
        ...(subscription.eventType === undefined
          ? {}
          : { eventType: expectOptionalString(subscription.eventType, `${path}.eventType`) }),
      };
    case "signalr":
      return {
        kind,
        hubUrl: expectString(subscription.hubUrl, `${path}.hubUrl`),
        method: expectString(subscription.method, `${path}.method`),
      };
  }
}

function decodeAction(value: unknown, path: string): PlanAction {
  const action = expectRecord(value, path);
  const kind = expectOneOf(
    action.kind,
    ["sequence", "branch", "parallel", "set", "call", "dispatch", "request", "inject", "show-validation-errors"],
    `${path}.kind`
  );

  switch (kind) {
    case "sequence":
      return {
        kind,
        steps: decodeArray(action.steps, `${path}.steps`, decodeAction),
      };
    case "branch":
      return {
        kind,
        cases: decodeArray(action.cases, `${path}.cases`, decodeBranchCase),
      };
    case "parallel":
      return {
        kind,
        steps: decodeArray(action.steps, `${path}.steps`, decodeAction),
        ...(action.onSettled === undefined
          ? {}
          : { onSettled: decodeAction(action.onSettled, `${path}.onSettled`) }),
      };
    case "set":
      return {
        kind,
        target: decodeActionTarget(action.target, `${path}.target`),
        value: decodeValueExpr(action.value, `${path}.value`),
      };
    case "call":
      return {
        kind,
        target: decodeActionTarget(action.target, `${path}.target`),
        ...(action.args === undefined
          ? {}
          : { args: decodeArray(action.args, `${path}.args`, decodeValueExpr) }),
      };
    case "dispatch":
      return {
        kind,
        name: expectString(action.name, `${path}.name`),
        ...(action.detail === undefined
          ? {}
          : { detail: decodeValueExpr(action.detail, `${path}.detail`) }),
      };
    case "request":
      return {
        kind,
        request: decodeRequest(action.request, `${path}.request`),
      };
    case "inject":
      return {
        kind,
        object: expectString(action.object, `${path}.object`),
        ...(action.value === undefined
          ? {}
          : { value: decodeValueExpr(action.value, `${path}.value`) }),
      };
    case "show-validation-errors":
      return {
        kind,
        formId: expectString(action.formId, `${path}.formId`),
      };
  }
}

function decodeBranchCase(value: unknown, path: string): BranchCase {
  const branch = expectRecord(value, path);
  return {
    ...(branch.when === undefined ? {} : { when: decodePredicate(branch.when, `${path}.when`) }),
    run: decodeAction(branch.run, `${path}.run`),
  };
}

function decodeActionTarget(value: unknown, path: string): ActionTarget {
  const target = expectRecord(value, path);
  return {
    object: expectString(target.object, `${path}.object`),
    member: expectString(target.member, `${path}.member`),
  };
}

function decodeRequest(value: unknown, path: string): RequestPlan {
  const request = expectRecord(value, path);
  return {
    method: expectOneOf(request.method, ["GET", "POST", "PUT", "DELETE"], `${path}.method`),
    url: expectString(request.url, `${path}.url`),
    ...(request.input === undefined ? {} : { input: decodeRequestInput(request.input, `${path}.input`) }),
    ...(request.validation === undefined
      ? {}
      : { validation: decodeValidation(request.validation, `${path}.validation`) }),
    ...(request.before === undefined
      ? {}
      : { before: decodeArray(request.before, `${path}.before`, decodeAction) }),
    ...(request.onSuccess === undefined
      ? {}
      : { onSuccess: decodeArray(request.onSuccess, `${path}.onSuccess`, decodeResponseHandler) }),
    ...(request.onError === undefined
      ? {}
      : { onError: decodeArray(request.onError, `${path}.onError`, decodeResponseHandler) }),
    ...(request.onSettled === undefined
      ? {}
      : { onSettled: decodeArray(request.onSettled, `${path}.onSettled`, decodeAction) }),
    ...(request.next === undefined ? {} : { next: decodeRequest(request.next, `${path}.next`) }),
  };
}

function decodeRequestInput(value: unknown, path: string): RequestInput {
  const input = expectRecord(value, path);
  return {
    transport: expectOneOf(input.transport, ["query", "json", "form-data"], `${path}.transport`),
    value: decodeRequestInputValue(input.value, `${path}.value`),
  };
}

function decodeRequestInputValue(value: unknown, path: string): RequestInputValue {
  const record = expectRecord(value, path);
  if (record.kind === "binding-map") {
    return decodeBindingMapValueExpr(record, path);
  }

  return decodeValueExpr(record, path);
}

function decodeBindingMapValueExpr(value: unknown, path: string): BindingMapValueExpr {
  const bindingMap = expectRecord(value, path);
  return {
    kind: "binding-map",
    include: bindingMap.include === "all"
      ? "all"
      : decodeArray(bindingMap.include, `${path}.include`, expectString),
  };
}

function decodeValidation(value: unknown, path: string): RequestValidation {
  const validation = expectRecord(value, path);
  return {
    formId: expectString(validation.formId, `${path}.formId`),
    fields: decodeArray(validation.fields, `${path}.fields`, decodeValidationField),
  };
}

function decodeValidationField(value: unknown, path: string): RequestValidationField {
  const field = expectRecord(value, path);
  return {
    binding: expectString(field.binding, `${path}.binding`),
    rules: decodeArray(field.rules, `${path}.rules`, decodeValidationRule),
  };
}

function decodeValidationRule(value: unknown, path: string): RequestValidationRule {
  const rule = expectRecord(value, path);
  return {
    rule: expectOneOf(
      rule.rule,
      [
        "required", "empty",
        "minLength", "maxLength", "email", "regex", "url", "creditCard",
        "range", "exclusiveRange",
        "min", "max", "gt", "lt",
        "equalTo", "notEqual", "notEqualTo",
        "atLeastOne",
      ] satisfies readonly ValidationRuleName[],
      `${path}.rule`
    ),
    message: expectString(rule.message, `${path}.message`),
    ...(rule.constraint === undefined ? {} : { constraint: rule.constraint }),
    ...(rule.otherBinding === undefined
      ? {}
      : { otherBinding: expectOptionalString(rule.otherBinding, `${path}.otherBinding`) }),
    ...(rule.as === undefined ? {} : { as: decodeShape(rule.as, `${path}.as`) }),
    ...(rule.when === undefined ? {} : { when: decodePredicate(rule.when, `${path}.when`) }),
  };
}

function decodeResponseHandler(value: unknown, path: string): ResponseHandlerPlan {
  const handler = expectRecord(value, path);
  return {
    ...(handler.statusCode === undefined ? {} : { statusCode: expectNumber(handler.statusCode, `${path}.statusCode`) }),
    run: decodeAction(handler.run, `${path}.run`),
  };
}

function decodePredicate(value: unknown, path: string): PlanPredicate {
  const predicate = expectRecord(value, path);
  const kind = expectOneOf(predicate.kind, ["compare", "all", "any", "not", "confirm"], `${path}.kind`);

  switch (kind) {
    case "compare":
      return {
        kind,
        left: decodeValueExpr(predicate.left, `${path}.left`),
        op: expectOneOf(
          predicate.op,
          [
            "eq", "neq", "gt", "gte", "lt", "lte",
            "truthy", "falsy", "is-null", "not-null",
            "is-empty", "not-empty",
            "in", "not-in", "between",
            "array-contains",
            "contains", "starts-with", "ends-with", "matches", "min-length",
          ] satisfies readonly CompareOp[],
          `${path}.op`
        ),
        ...(predicate.right === undefined ? {} : { right: decodeValueExpr(predicate.right, `${path}.right`) }),
        ...(predicate.as === undefined ? {} : { as: decodeShape(predicate.as, `${path}.as`) }),
        ...(predicate.itemAs === undefined ? {} : { itemAs: decodeShape(predicate.itemAs, `${path}.itemAs`) }),
      };
    case "all":
      return {
        kind,
        terms: decodeArray(predicate.terms, `${path}.terms`, decodePredicate),
      };
    case "any":
      return {
        kind,
        terms: decodeArray(predicate.terms, `${path}.terms`, decodePredicate),
      };
    case "not":
      return {
        kind,
        term: decodePredicate(predicate.term, `${path}.term`),
      };
    case "confirm":
      return {
        kind,
        message: expectString(predicate.message, `${path}.message`),
      };
  }
}

function decodeValueExpr(value: unknown, path: string): ValueExpr {
  const expr = expectRecord(value, path);
  const kind = expectOneOf(expr.kind, ["literal", "binding", "member", "context", "access", "object", "array", "convert"], `${path}.kind`);

  switch (kind) {
    case "literal":
      return Object.prototype.hasOwnProperty.call(expr, "value")
        ? { kind, value: expr.value }
        : { kind };
    case "binding":
      return {
        kind,
        binding: expectString(expr.binding, `${path}.binding`),
      };
    case "member":
      return {
        kind,
        object: expectString(expr.object, `${path}.object`),
        member: expectString(expr.member, `${path}.member`),
      };
    case "context":
      return {
        kind,
        scope: expectOneOf(expr.scope, ["event", "response", "request", "local"], `${path}.scope`),
        ...(expr.path === undefined ? {} : { path: decodeArray(expr.path, `${path}.path`, decodePathSegment) }),
      };
    case "access":
      return {
        kind,
        value: decodeValueExpr(expr.value, `${path}.value`),
        path: decodeArray(expr.path, `${path}.path`, decodePathSegment),
      };
    case "object":
      return {
        kind,
        fields: decodeMap(expr.fields, `${path}.fields`, decodeValueExpr),
      };
    case "array":
      return {
        kind,
        items: decodeArray(expr.items, `${path}.items`, decodeValueExpr),
      };
    case "convert":
      return {
        kind,
        value: decodeValueExpr(expr.value, `${path}.value`),
        to: decodeShape(expr.to, `${path}.to`),
      };
  }
}

function decodeShape(value: unknown, path: string): ValueShape {
  const shape = expectRecord(value, path);
  const kind = expectOneOf(shape.kind, ["scalar", "array", "object", "any"], `${path}.kind`);

  switch (kind) {
    case "scalar":
      return {
        kind,
        type: expectOneOf(shape.type, ["string", "number", "boolean", "date", "raw"], `${path}.type`),
      };
    case "array":
      return {
        kind,
        item: decodeShape(shape.item, `${path}.item`),
      };
    case "object":
      return {
        kind,
        ...(shape.fields === undefined ? {} : { fields: decodeMap(shape.fields, `${path}.fields`, decodeShape) }),
        ...(shape.additional === undefined ? {} : { additional: expectBoolean(shape.additional, `${path}.additional`) }),
      };
    case "any":
      return { kind };
  }
}

function decodePathSegment(value: unknown, path: string): PathSegment {
  const segment = expectRecord(value, path);
  const hasProp = Object.prototype.hasOwnProperty.call(segment, "prop");
  const hasIndex = Object.prototype.hasOwnProperty.call(segment, "index");

  if (hasProp === hasIndex) {
    throw new Error(`[alis] ${path} must contain exactly one of "prop" or "index"`);
  }

  if (hasProp) {
    return {
      prop: expectString(segment.prop, `${path}.prop`),
    };
  }

  return {
    index: expectNumber(segment.index, `${path}.index`),
  };
}

function decodeMap<T>(value: unknown, path: string, decodeValue: (value: unknown, path: string) => T): Record<string, T> {
  const record = expectRecord(value, path);
  return Object.fromEntries(
    Object.entries(record).map(([key, item]) => [key, decodeValue(item, `${path}.${key}`)])
  );
}

function decodeArray<T>(value: unknown, path: string, decodeValue: (value: unknown, path: string) => T): T[] {
  if (!Array.isArray(value)) {
    throw new Error(`[alis] ${path} must be an array`);
  }

  return value.map((item, index) => decodeValue(item, `${path}[${index}]`));
}

function expectRecord(value: unknown, path: string): Record<string, unknown> {
  if (typeof value !== "object" || value == null || Array.isArray(value)) {
    throw new Error(`[alis] ${path} must be an object`);
  }

  return value as Record<string, unknown>;
}

function expectString(value: unknown, path: string): string {
  if (typeof value !== "string" || value.length === 0) {
    throw new Error(`[alis] ${path} must be a non-empty string`);
  }

  return value;
}

function expectOptionalString(value: unknown, path: string): string {
  if (typeof value !== "string") {
    throw new Error(`[alis] ${path} must be a string when provided`);
  }

  return value;
}

function expectNumber(value: unknown, path: string): number {
  if (typeof value !== "number" || Number.isNaN(value)) {
    throw new Error(`[alis] ${path} must be a number`);
  }

  return value;
}

function expectExactNumber(value: unknown, expected: number, path: string): number {
  const actual = expectNumber(value, path);
  if (actual !== expected) {
    throw new Error(`[alis] ${path} must be ${expected}, got ${actual}`);
  }

  return actual;
}

function expectBoolean(value: unknown, path: string): boolean {
  if (typeof value !== "boolean") {
    throw new Error(`[alis] ${path} must be a boolean`);
  }

  return value;
}

function expectOneOf<T extends string>(value: unknown, allowed: readonly T[], path: string): T {
  const actual = expectString(value, path);
  if (!allowed.includes(actual as T)) {
    throw new Error(`[alis] ${path} must be one of ${allowed.join(", ")}, got ${actual}`);
  }

  return actual as T;
}
