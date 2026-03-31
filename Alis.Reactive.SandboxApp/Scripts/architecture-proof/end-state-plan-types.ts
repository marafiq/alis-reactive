import type { CoercionType as ValueShape } from "../core/coerce";
import type { Vendor } from "../types/context";

export interface EndStatePlan {
  planId: string;
  components: Record<string, ComponentEntry>;
  entries: Entry[];
}

export interface ComponentEntry {
  id: string;
  vendor: Vendor;
  componentType: string;
  value: ComponentValueDescriptor;
}

export interface ComponentValueDescriptor {
  path: string;
  shape?: ValueShape;
  elementShape?: ValueShape;
}

export interface Entry {
  trigger: Trigger;
  reaction: Reaction;
}

export type Trigger =
  | DomReadyTrigger
  | CustomEventTrigger
  | ComponentEventTrigger
  | ServerPushTrigger
  | SignalRTrigger;

export interface DomReadyTrigger {
  kind: "dom-ready";
}

export interface CustomEventTrigger {
  kind: "custom-event";
  event: string;
}

export interface ComponentEventTrigger {
  kind: "component-event";
  target: ComponentEventTarget;
  payload: ComponentEventPayload;
}

export interface ComponentEventTarget {
  componentId: string;
  vendor: Vendor;
  jsEvent: string;
}

export type ComponentEventPayload =
  | NoTriggerPayload
  | CallbackTriggerPayload
  | ObjectTriggerPayload;

export interface NoTriggerPayload {
  kind: "none";
}

export interface CallbackTriggerPayload {
  kind: "callback";
}

export interface ObjectTriggerPayload {
  kind: "object";
  fields: Record<string, TriggerProjectionValue>;
}

export type TriggerProjectionValue = LiteralValue | ComponentReadValue;

export interface ComponentReadValue {
  kind: "read";
  access: ComponentAccess;
}

export interface ServerPushTrigger {
  kind: "server-push";
  url: string;
  eventType?: string;
}

export interface SignalRTrigger {
  kind: "signalr";
  hubUrl: string;
  methodName: string;
}

export type Reaction =
  | SequentialReaction
  | ConditionalReaction
  | HttpReaction
  | ParallelHttpReaction;

export interface SequentialReaction {
  kind: "sequential";
  commands: Command[];
}

export interface ConditionalReaction {
  kind: "conditional";
  commands?: Command[];
  branches: Branch[];
}

export interface Branch {
  guard: Guard | null;
  reaction: Reaction;
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

export type Guard = ValueGuard | AllGuard | AnyGuard | InvertGuard | ConfirmGuard;

export type GuardOp =
  | "eq" | "neq"
  | "gt" | "gte" | "lt" | "lte"
  | "truthy" | "falsy"
  | "is-null" | "not-null"
  | "is-empty" | "not-empty"
  | "in" | "not-in" | "between"
  | "array-contains"
  | "contains" | "starts-with" | "ends-with" | "matches" | "min-length";

export interface ValueGuard {
  kind: "value";
  left: ValueAccess;
  op: GuardOp;
  right?: PlanValue;
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

export type Command =
  | DispatchCommand
  | MutateElementCommand
  | MutatePayloadCommand
  | ValidationErrorsCommand
  | IntoCommand;

export interface DispatchCommand {
  kind: "dispatch";
  event: string;
  payload?: ObjectValue;
}

export interface MutateElementCommand {
  kind: "mutate-element";
  target: string;
  mutation: Mutation;
  vendor?: Vendor;
}

export interface MutatePayloadCommand {
  kind: "mutate-payload";
  mutation: Mutation;
}

export interface ValidationErrorsCommand {
  kind: "validation-errors";
  formId: string;
}

export interface IntoCommand {
  kind: "into";
  target: string;
}

export type Mutation = SetPropMutation | CallMutation;

export interface SetPropMutation {
  kind: "set-prop";
  prop: string;
  value: PlanValue;
}

export interface CallMutation {
  kind: "call";
  method: string;
  chain?: string;
  args?: PlanValue[];
}

export type PlanValue =
  | LiteralValue
  | ReadValue
  | ObjectValue
  | ArrayValue;

export interface LiteralValue {
  kind: "literal";
  value: unknown;
  shape?: ValueShape;
}

export interface ReadValue {
  kind: "read";
  access: ValueAccess;
}

export interface ObjectValue {
  kind: "object";
  fields: Record<string, PlanValue>;
}

export interface ArrayValue {
  kind: "array";
  items: PlanValue[];
}

export interface ValueAccess {
  source: ValueSource;
  path: string;
  shape?: ValueShape;
  elementShape?: ValueShape;
}

export interface ComponentAccess {
  source: ComponentValueSource;
  path: string;
  shape?: ValueShape;
  elementShape?: ValueShape;
}

export type ValueSource = ComponentValueSource | PayloadValueSource;

export interface ComponentValueSource {
  kind: "component";
  componentId: string;
  vendor: Vendor;
}

export interface PayloadValueSource {
  kind: "payload";
  scope: "trigger" | "response";
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

export type GatherItem = AllGather | GatherField;

export interface AllGather {
  kind: "all";
}

export interface GatherField {
  kind: "field";
  name: string;
  value: PlanValue;
}

export interface StatusHandler {
  statusCode?: number;
  reaction: Reaction;
}

export interface ValidationDescriptor {
  formId: string;
  fields: ValidationField[];
}

export interface ValidationField {
  modelPath: string;
  rules: ValidationRule[];
}

export interface ValidationRule {
  rule: string;
  message: string;
  constraint?: unknown;
}
