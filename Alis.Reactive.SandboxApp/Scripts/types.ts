export interface Plan {
  planId: string;
  components: Record<string, ComponentEntry>;
  entries: Entry[];
  /** Set by inject.ts from container ID — used by mergePlan to deduplicate on partial reload. */
  sourceId?: string;
}

export interface ComponentEntry {
  id: string;
  vendor: Vendor;
  readExpr: string;
}

export interface Entry {
  trigger: Trigger;
  reaction: Reaction;
}

// -- Triggers -------------------------------------------------

export type Trigger = DomReadyTrigger | CustomEventTrigger | ComponentEventTrigger;

export type Vendor = "fusion" | "native";

export interface DomReadyTrigger {
  kind: "dom-ready";
}

export interface CustomEventTrigger {
  kind: "custom-event";
  event: string;
}

export interface ComponentEventTrigger {
  kind: "component-event";
  componentId: string;
  jsEvent: string;
  vendor: Vendor;
  bindingPath?: string;
  readExpr?: string;
}

// -- Reactions ------------------------------------------------

export type Reaction = SequentialReaction | ConditionalReaction | HttpReaction | ParallelHttpReaction;

export interface SequentialReaction {
  kind: "sequential";
  commands: Command[];
}

export interface ConditionalReaction {
  kind: "conditional";
  commands?: Command[];
  branches: Branch[];
}

export interface HttpReaction {
  kind: "http";
  preFetch?: Command[];
  request: RequestDescriptor;
}

export interface ParallelHttpReaction {
  kind: "parallel-http";
  preFetch?: Command[];
  requests: RequestDescriptor[];
  onAllSettled?: Command[];
}

// -- HTTP Request Types ----------------------------------------

export type GatherItem = ComponentGather | StaticGather | AllGather;

export interface AllGather {
  kind: "all";
}

export interface ComponentGather {
  kind: "component";
  componentId: string;
  vendor: Vendor;
  name: string;
  readExpr: string;
}

export interface StaticGather {
  kind: "static";
  param: string;
  value: unknown;
}

export interface StatusHandler {
  statusCode?: number;
  commands?: Command[];
  reaction?: Reaction;
}

export interface RequestDescriptor {
  verb: "GET" | "POST" | "PUT" | "DELETE";
  url: string;
  gather?: GatherItem[];
  contentType?: "form-data";
  whileLoading?: Command[];
  onSuccess?: StatusHandler[];
  onError?: StatusHandler[];
  chained?: RequestDescriptor;
  validation?: ValidationDescriptor;
}

// -- BindSource -----------------------------------------------

export type BindSource = EventSource | ComponentSource;

export interface EventSource {
  kind: "event";
  path: string;
}

export interface ComponentSource {
  kind: "component";
  componentId: string;
  vendor: Vendor;
  readExpr: string;
}

// -- Guards & Branches ----------------------------------------

export type GuardOp =
  | "eq" | "neq"
  | "gt" | "gte" | "lt" | "lte"
  | "truthy" | "falsy"
  | "is-null" | "not-null"
  | "is-empty" | "not-empty"
  | "in" | "not-in" | "between"
  | "array-contains"
  | "contains" | "starts-with" | "ends-with" | "matches" | "min-length";

export type Guard = ValueGuard | AllGuard | AnyGuard | InvertGuard | ConfirmGuard;

export interface ValueGuard {
  kind: "value";
  source: BindSource;
  coerceAs: "string" | "number" | "boolean" | "date" | "raw" | "array";
  op: GuardOp;
  operand?: unknown;
  rightSource?: BindSource;
  elementCoerceAs?: "string" | "number" | "boolean" | "date" | "raw" | "array";
}

export interface AllGuard {
  kind: "all";
  guards: Guard[];
}

export interface AnyGuard {
  kind: "any";
  guards: Guard[];
}

export interface InvertGuard {
  kind: "not";
  inner: Guard;
}

export interface ConfirmGuard {
  kind: "confirm";
  message: string;
}

export interface Branch {
  guard: Guard | null;
  reaction: Reaction;
}

// -- Commands -------------------------------------------------

export type Command = DispatchCommand | MutateElementCommand | ValidationErrorsCommand | IntoCommand;

export type EventPayload = Record<string, unknown>;

export interface DispatchCommand {
  kind: "dispatch";
  event: string;
  payload?: EventPayload;
  when?: Guard;
}

export type CoercionType = "string" | "number" | "boolean" | "date" | "raw" | "array";

// ── Mutation (discriminated by kind) ──

export type Mutation = SetPropMutation | CallMutation;

export interface SetPropMutation {
  kind: "set-prop";
  prop: string;
  coerce?: CoercionType;
}

export type MethodArg = LiteralArg | SourceArg;

export interface LiteralArg {
  kind: "literal";
  value: unknown;
}

export interface SourceArg {
  kind: "source";
  source: BindSource;
  coerce?: CoercionType;
}

export interface CallMutation {
  kind: "call";
  method: string;
  chain?: string;
  args?: MethodArg[];
}

export interface MutateElementCommand {
  kind: "mutate-element";
  target: string;
  mutation: Mutation;
  value?: string | string[];
  source?: BindSource;
  vendor?: Vendor;
  when?: Guard;
}

export interface ValidationErrorsCommand {
  kind: "validation-errors";
  formId: string;
  when?: Guard;
}

export interface IntoCommand {
  kind: "into";
  target: string;
  when?: Guard;
}

// -- Execution Context ----------------------------------------

export interface ExecContext {
  evt?: Record<string, unknown>;
  responseBody?: unknown;
  validationDesc?: ValidationDescriptor;
  components?: Record<string, ComponentEntry>;
}

// -- Validation -----------------------------------------------

export interface ValidationDescriptor {
  formId: string;
  planId?: string;
  fields: ValidationField[];
}

export interface ValidationField {
  fieldName: string;
  rules: ValidationRule[];
  // Enriched at boot from plan.components:
  fieldId?: string;
  vendor?: Vendor;
  readExpr?: string;
}

export type ValidationRuleType =
  | "required" | "minLength" | "maxLength" | "email" | "regex" | "url"
  | "range" | "min" | "max" | "equalTo" | "atLeastOne";

export interface ValidationRule {
  rule: ValidationRuleType;
  message: string;
  constraint?: boolean | number | string | [number, number];
  when?: ValidationCondition;
}

export interface ValidationCondition {
  field: string;
  op: "truthy" | "falsy" | "eq" | "neq";
  value?: unknown;
}
