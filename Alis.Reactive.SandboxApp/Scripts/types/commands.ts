import type { BindSource } from "./sources";
import type { Vendor, CoercionType, EventPayload } from "./context";
import type { Guard } from "./guards";

export type Command = DispatchCommand | MutateElementCommand | ValidationErrorsCommand | IntoCommand;

export interface DispatchCommand {
  kind: "dispatch";
  event: string;
  payload?: EventPayload;
  when?: Guard;
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
