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

export type Reaction = SequentialReaction;

export interface SequentialReaction {
  kind: "sequential";
  commands: Command[];
}

// -- Commands -------------------------------------------------

export type Command = DispatchCommand | MutateElementCommand;

export type EventPayload = Record<string, unknown>;

export interface DispatchCommand {
  kind: "dispatch";
  event: string;
  payload?: EventPayload;
}

export type MutateAction =
  | "add-class"
  | "remove-class"
  | "toggle-class"
  | "set-text"
  | "set-html"
  | "show"
  | "hide";

export interface MutateElementCommand {
  kind: "mutate-element";
  target: string;
  action: MutateAction;
  value?: string;
}
