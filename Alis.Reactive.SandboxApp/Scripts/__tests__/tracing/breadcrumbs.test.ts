import { describe, it, expect } from "vitest";
import { BreadcrumbBuffer } from "../../tracing/breadcrumbs";
import type { Breadcrumb } from "../../tracing/types";

function crumb(event: string): Breadcrumb {
  return { time: performance.now(), event, scope: "test", level: "info" };
}

describe("BreadcrumbBuffer", () => {
  it("returns empty snapshot when no items pushed", () => {
    const buf = new BreadcrumbBuffer(4);
    expect(buf.snapshot()).toEqual([]);
  });

  it("returns items in push order", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.push(crumb("c"));
    const snap = buf.snapshot();
    expect(snap.map(b => b.event)).toEqual(["a", "b", "c"]);
  });

  it("overwrites oldest when capacity exceeded", () => {
    const buf = new BreadcrumbBuffer(3);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.push(crumb("c"));
    buf.push(crumb("d"));
    const snap = buf.snapshot();
    expect(snap.map(b => b.event)).toEqual(["b", "c", "d"]);
  });

  it("wraps around multiple times", () => {
    const buf = new BreadcrumbBuffer(2);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.push(crumb("c"));
    buf.push(crumb("d"));
    buf.push(crumb("e"));
    const snap = buf.snapshot();
    expect(snap.map(b => b.event)).toEqual(["d", "e"]);
  });

  it("clear resets buffer", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.clear();
    expect(buf.snapshot()).toEqual([]);
  });

  it("works after clear and re-push", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    buf.clear();
    buf.push(crumb("b"));
    expect(buf.snapshot().map(b => b.event)).toEqual(["b"]);
  });
});
