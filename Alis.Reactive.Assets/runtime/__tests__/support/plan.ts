// Small builders for plans in tests. They write the same JSON the C# grammar emits.

import type {
  Expr, FnExpr, Handler, ObjectLocator, Operator, PlanDocument, Shape, Statement, Vendor,
} from "../../types/index";

export const s = {
  string: { kind: "string" } as Shape,
  number: { kind: "number" } as Shape,
  boolean: { kind: "boolean" } as Shape,
  date: { kind: "date" } as Shape,
  instant: { kind: "instant" } as Shape,
  any: { kind: "any" } as Shape,
  none: { kind: "none" } as Shape,
  array: (item: Shape): Shape => ({ kind: "array", item }),
  nullable: (inner: Shape): Shape => ({ kind: "nullable", inner }),
  object: (fields: Record<string, Shape>): Shape => ({ kind: "object", fields }),
};

export const e = {
  lit: (value: unknown): Expr => ({ kind: "lit", value: value as never }),
  as: (expr: Expr, shape: Shape): Expr => ({ kind: "as", expr, shape }),
  obj: (id: string): Expr => ({ kind: "obj", id }),
  ref: (name: string): Expr => ({ kind: "ref", name }),
  get: (target: Expr, path: string[]): Expr => ({ kind: "get", target, path }),
  call: (target: Expr, path: string[], args: Expr[] = []): Expr => ({ kind: "call", target, path, args }),
  fn: (params: string[], body: Expr): FnExpr => ({ kind: "fn", params, body }),
  record: (fields: Record<string, Expr>): Expr => ({
    kind: "record",
    fields: Object.entries(fields).map(([name, value]) => ({ name, value })),
  }),
  list: (...items: Expr[]): Expr => ({ kind: "list", items }),
  op: (op: Operator, ...args: Expr[]): Expr => ({ kind: "op", op, args }),
};

export const st = {
  set: (target: Expr, path: string[], value: Expr): Statement => ({ kind: "set", target, path, value }),
  do: (expr: Expr, awaits = false): Statement => ({ kind: "do", expr, await: awaits }),
  let: (name: string, expr: Expr, awaits = false): Statement => ({ kind: "let", name, expr, await: awaits }),
  if: (arms: [Expr, Statement[]][], otherwise: Statement[] = []): Statement =>
    ({ kind: "if", arms: arms.map(([when, then]) => ({ when, then })), else: otherwise }),
  all: (...blocks: Statement[][]): Statement => ({ kind: "all", blocks }),
  try: (body: Statement[], cleanup: Statement[]): Statement => ({ kind: "try", body, finally: cleanup }),
};

export const vendors: Record<string, Vendor> = {
  native: {
    objectPath: [],
    listen: { kind: "method", subscribe: "addEventListener", unsubscribe: "removeEventListener" },
    payload: "element",
  },
  fake: { objectPath: ["widget"], listen: { kind: "method", subscribe: "on", unsubscribe: "off" }, payload: "argument" },
  // The same fake widget, listened to by assignment: its events are callback properties (`widget.onPressed = fn`).
  assigned: { objectPath: ["widget"], listen: { kind: "assignment" }, payload: "argument" },
};

export function element(id: string, vendor = "native"): ObjectLocator {
  return { kind: "element", id, vendor };
}

export function plugin(name: string, key = "", data: unknown = { kind: "none" }): ObjectLocator {
  return { kind: "plugin", name, key, data: data as never };
}

export function plan(
  objects: Record<string, ObjectLocator>,
  handlers: Handler[],
  planId = "Test.Plan",
): PlanDocument {
  return { version: 5, planId, vendors, objects, handlers };
}

/** A handler; its block reads the payload as `e`. */
export function on(object: string, event: string, ...statements: Statement[]): Handler {
  return { on: object, path: [], event, do: statements };
}

/** An element whose vendor object is a tiny event emitter under `widget`, like a Syncfusion instance. */
export function fakeWidget(id: string, members: Record<string, unknown> = {}): HTMLElement & { widget: FakeWidget } {
  const el = document.createElement("div") as unknown as HTMLElement & { widget: FakeWidget };
  el.id = id;
  el.widget = new FakeWidget(members);
  document.body.appendChild(el);
  return el;
}

export class FakeWidget {
  private readonly listeners = new Map<string, Set<(arg: unknown) => void>>();
  [member: string]: unknown;

  constructor(members: Record<string, unknown>) {
    Object.assign(this, members);
  }

  on(event: string, listener: (arg: unknown) => void): void {
    if (!this.listeners.has(event)) this.listeners.set(event, new Set());
    this.listeners.get(event)!.add(listener);
  }

  off(event: string, listener: (arg: unknown) => void): void {
    this.listeners.get(event)?.delete(listener);
  }

  trigger(event: string, arg: unknown): void {
    for (const listener of this.listeners.get(event) ?? []) listener(arg);
  }

  /** Calls the function assigned to a callback property, the way a widget calls its `onPressed`. */
  invoke(callback: string, arg: unknown): void {
    const assigned = this[callback];
    if (typeof assigned === "function") assigned.call(this, arg);
  }

  listenerCount(event: string): number {
    return this.listeners.get(event)?.size ?? 0;
  }
}
