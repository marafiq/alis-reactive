import { describe, expect, it } from "vitest";
import { BreadcrumbBuffer } from "../../tracing/breadcrumbs";
import type { Breadcrumb } from "../../tracing/types";

function crumb(event: string, time = 0): Breadcrumb {
  return { time, event, scope: "test", level: "info" };
}

describe("BreadcrumbBuffer constructor", () => {
  it("throws on zero capacity", () => {
    expect(() => new BreadcrumbBuffer(0)).toThrow(RangeError);
  });

  it("throws on negative capacity", () => {
    expect(() => new BreadcrumbBuffer(-1)).toThrow(RangeError);
  });

  it("throws on non-integer capacity", () => {
    expect(() => new BreadcrumbBuffer(1.5)).toThrow(RangeError);
  });
});

describe("BreadcrumbBuffer.push + snapshot", () => {
  it("snapshot is empty before any push", () => {
    const buf = new BreadcrumbBuffer(4);
    expect(buf.snapshot()).toEqual([]);
    expect(buf.size).toBe(0);
  });

  it("preserves insertion order while under capacity", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.push(crumb("c"));
    expect(buf.snapshot().map((c) => c.event)).toEqual(["a", "b", "c"]);
    expect(buf.size).toBe(3);
  });

  it("fills to exact capacity without drops", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.push(crumb("c"));
    buf.push(crumb("d"));
    expect(buf.snapshot().map((c) => c.event)).toEqual(["a", "b", "c", "d"]);
    expect(buf.size).toBe(4);
  });

  it("drops the oldest entry when capacity is exceeded", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.push(crumb("c"));
    buf.push(crumb("d"));
    buf.push(crumb("e"));
    expect(buf.snapshot().map((c) => c.event)).toEqual(["b", "c", "d", "e"]);
    expect(buf.size).toBe(4);
  });

  it("wraps multiple times without losing order", () => {
    const buf = new BreadcrumbBuffer(3);
    for (const e of ["a", "b", "c", "d", "e", "f", "g"]) {
      buf.push(crumb(e));
    }
    expect(buf.snapshot().map((c) => c.event)).toEqual(["e", "f", "g"]);
    expect(buf.size).toBe(3);
  });

  it("snapshot returns a fresh copy, not a live view", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    const first = buf.snapshot();
    buf.push(crumb("b"));
    const second = buf.snapshot();
    expect(first.map((c) => c.event)).toEqual(["a"]);
    expect(second.map((c) => c.event)).toEqual(["a", "b"]);
  });

  it("carries the full Breadcrumb shape through", () => {
    const buf = new BreadcrumbBuffer(2);
    const entry: Breadcrumb = {
      time: 123.4,
      event: "http.request.send",
      scope: "http",
      level: "debug",
      data: { method: "GET", url: "/api/x" },
    };
    buf.push(entry);
    const snap = buf.snapshot();
    expect(snap).toHaveLength(1);
    expect(snap[0]).toEqual(entry);
  });
});
