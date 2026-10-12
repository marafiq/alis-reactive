// The grammar's meaning (grammar.md sections 1-3): the browser computes what the developer's C# would.

import { afterEach, describe, expect, it } from "vitest";
import { Environment } from "../grammar/environment";
import { evaluate, type Scope, type World } from "../grammar/evaluate";
import { executeBlock } from "../grammar/execute";
import { e, s, st } from "./support/plan";

function scopeWith(objects: Record<string, unknown> = {}, bindings: Record<string, unknown> = {}): Scope {
  const env = Environment.root();
  for (const [name, value] of Object.entries(bindings)) env.bind(name, value);
  const world: World = {
    object: id => {
      if (!(id in objects)) throw new Error(`no object ${id}`);
      return objects[id];
    },
  };
  return { env, world };
}

const value = (expr: Parameters<typeof evaluate>[0], scope = scopeWith()) => evaluate(expr, scope);

afterEach(() => {
  document.body.innerHTML = "";
});

describe("values convert at the boundary, by shape", () => {
  it("reads text as a number the way MVC binds a number, and keeps what cannot convert", () => {
    expect(value(e.lit("42", s.number))).toBe(42);
    expect(value(e.lit("", s.number))).toBe(0);
    expect(value(e.lit("abc", s.number))).toBe("abc");
  });

  it("reads a nullable number's blank text as null", () => {
    expect(value(e.lit("  ", s.nullable(s.number)))).toBeNull();
    expect(value(e.lit(undefined, s.nullable(s.string)))).toBeNull();
  });

  it("reads a date-only text as local midnight", () => {
    const date = value(e.lit("2026-06-01", s.date)) as Date;
    expect(date).toBeInstanceOf(Date);
    expect([date.getFullYear(), date.getMonth(), date.getDate(), date.getHours()]).toEqual([2026, 5, 1, 0]);
  });

  it("reads an instant as the exact moment its text names, and writes it back in UTC", () => {
    const arrives = value(e.lit("2026-10-11T10:30:00+05:00", s.instant)) as Date;
    expect(arrives.toISOString()).toBe("2026-10-11T05:30:00.000Z");
    expect(value(e.op("toText", e.lit("2026-10-11T10:30:00+05:00", s.instant)))).toBe("2026-10-11T05:30:00.000Z");
    expect(value(e.op("eq", e.lit("2026-10-11T05:30:00Z", s.instant), e.lit(Date.UTC(2026, 9, 11, 5, 30), s.date)))).toBe(true);
  });

  it("keeps a missing value missing", () => {
    expect(value(e.lit(null, s.number))).toBeNull();
    expect(value(e.lit(undefined, s.string))).toBeUndefined();
  });
});

describe("operators answer as C#'s lifted operators do", () => {
  it("treats null and undefined as one missing value", () => {
    expect(value(e.op("eq", e.lit(null), e.lit(undefined)))).toBe(true);
    expect(value(e.op("eq", e.lit(null), e.lit(0)))).toBe(false);
  });

  it("never orders a missing value, NaN or mixed kinds", () => {
    expect(value(e.op("lt", e.lit(null), e.lit(15)))).toBe(false);
    expect(value(e.op("gte", e.lit(null), e.lit(0)))).toBe(false);
    expect(value(e.op("gt", e.lit("10"), e.lit(9)))).toBe(false);
    expect(value(e.op("gt", e.lit(10), e.lit(9)))).toBe(true);
  });

  it("orders dates by their time and compares them for equality by time", () => {
    const earlier = e.lit("2026-01-01", s.date);
    const later = e.lit("2026-02-01", s.date);
    expect(value(e.op("lt", earlier, later))).toBe(true);
    expect(value(e.op("eq", e.lit("2026-01-01", s.date), e.lit("2026-01-01", s.date)))).toBe(true);
  });

  it("short-circuits && and || left to right", () => {
    const throws = e.call(e.ref("boom"), []);
    const scope = scopeWith({}, { boom: () => { throw new Error("evaluated"); } });
    expect(value(e.op("and", e.lit(false), throws), scope)).toBe(false);
    expect(value(e.op("or", e.lit(true), throws), scope)).toBe(true);
  });

  it("treats only \"\", missing and an empty array as empty", () => {
    expect(value(e.op("empty", e.lit("")))).toBe(true);
    expect(value(e.op("empty", e.lit([])))).toBe(true);
    expect(value(e.op("empty", e.lit(" ")))).toBe(false);
    expect(value(e.op("empty", e.lit(0)))).toBe(false);
  });

  it("tests a pattern on text and never on a missing value", () => {
    expect(value(e.op("matches", e.lit("A-12"), e.lit("^[A-Z]-\\d+$"), e.lit("")))).toBe(true);
    expect(value(e.op("matches", e.lit(undefined), e.lit(".*"), e.lit("")))).toBe(false);
  });

  it("sums with text that is not a number as 0, never concatenating", () => {
    const rows = e.lit([{ n: 2 }, { n: "3" }, { n: "x" }]);
    const sum = e.call(e.op("toArray", rows), ["reduce"], [
      e.fn(["total", "row"], e.op("add", e.ref("total"), e.op("finite", e.get(e.ref("row"), ["n"])))),
      e.lit(0),
    ]);
    expect(value(sum)).toBe(5);
  });

  it("orders numbers by value with non-finite last, and everything else by text", () => {
    const sorted = e.call(e.lit([3, Infinity, 1, 2]), ["toSorted"], [e.fn(["a", "b"], e.op("order", e.ref("a"), e.ref("b")))]);
    expect(value(sorted)).toEqual([1, 2, 3, Infinity]);
  });
});

describe("paths", () => {
  it("fails loudly on a member a located object does not have", () => {
    const scope = scopeWith({ grid: { dataSource: [] } });
    expect(() => value(e.get(e.obj("grid"), ["pageSize"]), scope)).toThrow(/has no member "pageSize"/);
  });

  it("reads a missing link of data as missing, as JavaScript's ?. does", () => {
    const scope = scopeWith({}, { args: { data: undefined } });
    expect(value(e.get(e.ref("args"), ["data", "id"]), scope)).toBeUndefined();
    expect(value(e.call(e.ref("args"), ["data", "includes"], [e.lit("x")]), scope)).toBeUndefined();
  });

  it("creates a payload member that is absent, but never a member a located object lacks", () => {
    const args: Record<string, unknown> = {};
    const scope = scopeWith({ widget: {} }, { args });
    executeBlock([st.set(e.ref("args"), ["cancel"], e.lit(true), s.boolean)], scope);
    expect(args.cancel).toBe(true);
    expect(() => executeBlock([st.set(e.obj("widget"), ["value"], e.lit(1))], scope)).toThrow(/has no member "value"/);
  });

  it("calls a text method on a string receiver", () => {
    const scope = scopeWith({}, { name: "Margaret Hill" });
    expect(value(e.call(e.ref("name"), ["startsWith"], [e.lit("Marg")]), scope)).toBe(true);
  });
});

describe("statements", () => {
  it("writes the value converted to the member's shape", () => {
    const el = document.createElement("p");
    const scope = scopeWith({ status: el });
    executeBlock([st.set(e.obj("status"), ["textContent"], e.lit(5), s.string)], scope);
    expect(el.textContent).toBe("5");
  });

  it("runs synchronously until the first awaited statement", async () => {
    const events: string[] = [];
    let release!: () => void;
    const slow = new Promise<void>(resolve => { release = resolve; });
    const scope = scopeWith({}, {
      log: (entry: string) => events.push(entry),
      wait: () => slow,
    });
    const completion = executeBlock([
      st.do(e.call(e.ref("log"), [], [e.lit("before")])),
      st.do(e.call(e.ref("wait"), []), true),
      st.do(e.call(e.ref("log"), [], [e.lit("after")])),
    ], scope);

    expect(events).toEqual(["before"]);
    release();
    await completion;
    expect(events).toEqual(["before", "after"]);
  });

  it("never returns a promise from a block that awaits nothing", () => {
    const scope = scopeWith({}, { noop: () => undefined });
    expect(executeBlock([st.do(e.call(e.ref("noop"), []))], scope)).toBeUndefined();
  });

  it("binds an awaited value for the rest of the block, and keeps a try body's bindings after it", async () => {
    const seen: unknown[] = [];
    const scope = scopeWith({}, {
      fetchRow: () => Promise.resolve({ id: 7 }),
      record: (v: unknown) => seen.push(v),
    });
    await executeBlock([
      st.try([st.let("res", e.call(e.ref("fetchRow"), []), true)], []),
      st.do(e.call(e.ref("record"), [], [e.get(e.ref("res"), ["id"])])),
    ], scope);
    expect(seen).toEqual([7]);
  });

  it("runs finally after the body, and after a failure before rethrowing it", async () => {
    const events: string[] = [];
    const scope = scopeWith({}, {
      log: (entry: string) => events.push(entry),
      fail: () => Promise.reject(new Error("no route value")),
    });
    await expect(executeBlock([
      st.try(
        [st.do(e.call(e.ref("fail"), []), true), st.do(e.call(e.ref("log"), [], [e.lit("unreached")]))],
        [st.do(e.call(e.ref("log"), [], [e.lit("finally")]))],
      ),
    ], scope)).rejects.toThrow("no route value");
    expect(events).toEqual(["finally"]);
  });

  it("takes the first arm whose condition holds, else the else block", () => {
    const events: string[] = [];
    const scope = scopeWith({}, { log: (entry: string) => events.push(entry) });
    const say = (text: string) => st.do(e.call(e.ref("log"), [], [e.lit(text)]));
    executeBlock([st.if([[e.lit(false), [say("a")]], [e.lit(true), [say("b")]], [e.lit(true), [say("c")]]], [say("else")])], scope);
    executeBlock([st.if([[e.lit(null), [say("a")]]], [say("else")])], scope);
    expect(events).toEqual(["b", "else"]);
  });

  it("settles every parallel block, logging a failure without stopping the others", async () => {
    const events: string[] = [];
    const scope = scopeWith({}, {
      log: (entry: string) => events.push(entry),
      later: (entry: string) => Promise.resolve().then(() => { events.push(entry); }),
      fail: () => Promise.reject(new Error("one branch failed")),
    });
    await executeBlock([
      st.all(
        [st.do(e.call(e.ref("fail"), []), true)],
        [st.do(e.call(e.ref("later"), [], [e.lit("second")]), true)],
      ),
      st.do(e.call(e.ref("log"), [], [e.lit("completed")])),
    ], scope);
    expect(events).toEqual(["second", "completed"]);
  });

  it("shapes an awaited result when it arrives", async () => {
    const seen: unknown[] = [];
    const scope = scopeWith({}, {
      census: () => Promise.resolve("12"),
      record: (v: unknown) => seen.push(v),
    });
    await executeBlock([
      st.let("count", e.call(e.ref("census"), [], [], s.number), true),
      st.do(e.call(e.ref("record"), [], [e.ref("count")])),
    ], scope);
    expect(seen).toEqual([12]);
  });

  it("refuses a promise from a member the plan reads as a value, naming the member", () => {
    const planner = { isBooked: () => Promise.resolve(false), status: "" };
    const scope = scopeWith({ planner });
    const isBooked = e.call(e.obj("planner"), ["isBooked"], [e.lit("sat-11")]);

    expect(() => executeBlock([st.if([[isBooked, [st.set(e.obj("planner"), ["status"], e.lit("booked"))]]])], scope))
      .toThrow("[alis] #planner.isBooked returned a promise where the plan reads a value; declare the member Awaited");
    expect(() => executeBlock([st.set(e.obj("planner"), ["status"], isBooked)], scope)).toThrow("declare the member Awaited");
    expect(() => executeBlock([st.let("booked", isBooked)], scope)).toThrow("declare the member Awaited");
    expect(planner.status).toBe("");
  });

  it("lets a statement call a member that returns a promise without waiting for it", () => {
    const scope = scopeWith({ grid: { refresh: () => Promise.resolve() } });
    expect(executeBlock([st.do(e.call(e.obj("grid"), ["refresh"], []))], scope)).toBeUndefined();
  });
});
