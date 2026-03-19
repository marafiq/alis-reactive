import type { Vendor } from "./context";

export type Trigger = DomReadyTrigger | CustomEventTrigger | ComponentEventTrigger;

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
