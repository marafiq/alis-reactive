import type { BindSource } from "./sources";
import type { Vendor } from "./context";
import type { CoercionType } from "../core/coerce";

export type Command = DispatchCommand | MutateElementCommand | MutateEventCommand | ValidationErrorsCommand | IntoCommand;

export interface DispatchCommand {
  kind: "dispatch";
  event: string;
  payload?: DispatchPayload;
}

// ── Mutation (discriminated by kind) ──

export type Mutation = SetPropMutation | CallMutation;

export interface SetPropMutation {
  kind: "set-prop";
  prop: string;
  value: CommandValue;
}

export type CommandValue = LiteralValue | SourceValue;

export type DispatchPayload = Record<string, CommandValue>;

export interface LiteralValue {
  kind: "literal";
  value: unknown;
  coerce?: CoercionType;
}

export interface SourceValue {
  kind: "source";
  source: BindSource;
  coerce?: CoercionType;
}

export interface CallMutation {
  kind: "call";
  method: string;
  chain?: string;
  args?: CommandValue[];
}

export interface MutateElementCommand {
  kind: "mutate-element";
  target: string;
  mutation: Mutation;
  vendor?: Vendor;
}

export interface ValidationErrorsCommand {
  kind: "validation-errors";
  formId: string;
}

export interface MutateEventCommand {
  kind: "mutate-event";
  mutation: Mutation;
}

export interface IntoCommand {
  kind: "into";
  target: string;
}
