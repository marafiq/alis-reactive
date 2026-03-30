import type { BindSource } from "./sources";
import type { Vendor, EventPayload } from "./context";
import type { CoercionType } from "../core/coerce";

export type Command = DispatchCommand | MutateElementCommand | MutateEventCommand | ValidationErrorsCommand | IntoCommand;

export interface DispatchCommand {
  kind: "dispatch";
  event: string;
  payload?: EventPayload;
}

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
}

export interface ValidationErrorsCommand {
  kind: "validation-errors";
  formId: string;
}

export interface MutateEventCommand {
  kind: "mutate-event";
  mutation: Mutation;
  value?: string | string[];
  source?: BindSource;
}

export interface IntoCommand {
  kind: "into";
  target: string;
}
