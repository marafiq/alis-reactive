import type { Vendor } from "./context";
import type { Command } from "./commands";
import type { Reaction } from "./reactions";
import type { ValidationDescriptor } from "./validation";

export type GatherItem = ComponentGather | StaticGather | AllGather;

export interface AllGather {
  kind: "all";
}

export interface ComponentGather {
  kind: "component";
  componentId: string;
  vendor: Vendor;
  name: string;
  readExpr: string;
}

export interface StaticGather {
  kind: "static";
  param: string;
  value: unknown;
}

export interface StatusHandler {
  statusCode?: number;
  commands?: Command[];
  reaction?: Reaction;
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
