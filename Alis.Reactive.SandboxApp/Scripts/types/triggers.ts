import type { Vendor } from "./context";

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
  componentId: string;
  jsEvent: string;
  vendor: Vendor;
  bindingPath?: string;
  readExpr?: string;
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
