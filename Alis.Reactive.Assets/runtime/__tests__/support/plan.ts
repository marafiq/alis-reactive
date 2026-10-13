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
  lit: (value: unknown, shape: Shape = s.any): Expr => ({ kind: "lit", value: value as never, shape }),
  obj: (id: string): Expr => ({ kind: "obj", id }),
  ref: (name: string): Expr => ({ kind: "ref", name }),
  get: (target: Expr, path: string[], shape: Shape = s.any): Expr => ({ kind: "get", target, path, shape }),
  call: (target: Expr, path: string[], args: Expr[] = [], shape: Shape = s.any, argShapes: Shape[] = []): Expr =>
    ({ kind: "call", target, path, args, argShapes, shape }),
  fn: (params: string[], body: Expr): FnExpr => ({ kind: "fn", params, body }),
  record: (fields: Record<string, Expr>, spreads: Expr[] = []): Expr => ({
    kind: "record",
    entries: [
      ...spreads.map(value => ({ kind: "spread" as const, value })),
      ...Object.entries(fields).map(([name, value]) => ({ kind: "field" as const, name, value })),
    ],
  }),
  list: (...items: Expr[]): Expr => ({ kind: "list", items }),
  op: (op: Operator, ...args: Expr[]): Expr => ({ kind: "op", op, args }),
};

export const st = {
  set: (target: Expr, path: string[], value: Expr, shape: Shape = s.any): Statement =>
    ({ kind: "set", target, path, shape, value }),
  do: (expr: Expr, awaits = false): Statement => ({ kind: "do", expr, await: awaits }),
  let: (name: string, expr: Expr, awaits = false): Statement => ({ kind: "let", name, expr, await: awaits }),
  if: (arms: [Expr, Statement[]][], otherwise: Statement[] = []): Statement =>
    ({ kind: "if", arms: arms.map(([when, then]) => ({ when, then })), else: otherwise }),
  all: (...blocks: Statement[][]): Statement => ({ kind: "all", blocks }),
  try: (body: Statement[], cleanup: Statement[]): Statement => ({ kind: "try", body, finally: cleanup }),
};

export const vendors: Record<string, Vendor> = {
  dom: { objectPath: [], subscribe: "addEventListener", unsubscribe: "removeEventListener", payload: "event" },
  native: { objectPath: [], subscribe: "addEventListener", unsubscribe: "removeEventListener", payload: "element" },
  fake: { objectPath: ["widget"], subscribe: "on", unsubscribe: "off", payload: "argument" },
};

export function element(id: string, vendor = "dom"): ObjectLocator {
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
  return { version: 4, planId, vendors, objects, handlers };
}

export function on(object: string, event: string, param: string, ...statements: Statement[]): Handler {
  return { on: object, event, param, do: statements };
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

  listenerCount(event: string): number {
    return this.listeners.get(event)?.size ?? 0;
  }
}
