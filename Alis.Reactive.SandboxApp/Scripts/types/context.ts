import type { Component } from "./plan";

export type EventPayload = Record<string, unknown>;

export interface ExecContext {
  readonly planId: string;
  readonly types: import("./plan").Plan["types"];
  readonly components: Record<string, Component>;
  readonly payload?: Record<string, unknown>;
}
