export interface Plan {
  entries: Entry[];
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
}

// -- Reactions ------------------------------------------------

export type Reaction = SequentialReaction | ConditionalReaction;

export interface SequentialReaction {
  kind: "sequential";
  commands: Command[];
}

export interface ConditionalReaction {
  kind: "conditional";
  branches: Branch[];
}

// -- BindSource -----------------------------------------------

export type BindSource = EventSource;
// Future: export type BindSource = EventSource | ComponentSource;

export interface EventSource {
  kind: "event";
  path: string;
}

// -- Guards & Branches ----------------------------------------

export type GuardOp =
  | "eq" | "neq"
  | "gt" | "gte" | "lt" | "lte"
  | "truthy" | "falsy"
  | "is-null" | "not-null"
  | "is-empty" | "not-empty"
  | "in" | "not-in" | "between"
  | "contains" | "starts-with" | "ends-with" | "matches" | "min-length";

export type Guard = ValueGuard | AllGuard | AnyGuard | InvertGuard | ConfirmGuard;

export interface ValueGuard {
  kind: "value";
  source: BindSource;
  coerceAs: "string" | "number" | "boolean" | "date" | "raw";
  op: GuardOp;
  operand?: unknown;
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

export type Command = DispatchCommand | MutateElementCommand;

export type EventPayload = Record<string, unknown>;

export interface DispatchCommand {
  kind: "dispatch";
  event: string;
  payload?: EventPayload;
  when?: Guard;
}

export interface MutateElementCommand {
  kind: "mutate-element";
  target: string;
  jsEmit: string;
  value?: string;
  source?: string;
  when?: Guard;
}

// -- Execution Context ----------------------------------------

export interface ExecContext {
  evt?: Record<string, unknown>;
}
