import type { Vendor } from "./context";

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
