export interface Plan {
  entries: Entry[];
}

export interface Entry {
  trigger: Trigger;
  reaction: Reaction;
}

// -- Triggers -------------------------------------------------

export type Trigger = DomReadyTrigger | CustomEventTrigger;

export interface DomReadyTrigger {
  kind: "dom-ready";
}

export interface CustomEventTrigger {
  kind: "custom-event";
  event: string;
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

// -- Guards & Branches ----------------------------------------

export type GuardOp =
  | "eq" | "neq"
  | "gt" | "gte" | "lt" | "lte"
  | "truthy" | "falsy"
  | "is-null" | "not-null";

export type Guard = ValueGuard | AllGuard | AnyGuard;

export interface ValueGuard {
  kind: "value";
  source: string;
  coerceAs: "string" | "number" | "boolean" | "raw";
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
}

export interface MutateElementCommand {
  kind: "mutate-element";
  target: string;
  jsEmit: string;
  value?: string;
  source?: string;
}

// -- Execution Context ----------------------------------------

export interface ExecContext {
  evt?: Record<string, unknown>;
}
