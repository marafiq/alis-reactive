import { describe, expect, test, vi } from "vitest";

async function aPage() {
  vi.resetModules();
  return import("./index");
}

describe("the page's plugin queue", () => {
  test("a plugin pushed before boot is found as its instance", async () => {
    const { readQueue, plugin } = await aPage();
    const queue = [{ name: "shift-log", instance: { entries: 3 } }];

    readQueue(queue);

    expect(plugin("shift-log", "")).toStrictEqual({ entries: 3 });
    expect(plugin("shift-log", "")).toBe(queue[0]?.instance);
  });

  test("a push after boot provides every instance at once and keeps the queue's length", async () => {
    const { readQueue, plugin } = await aPage();
    const queue = [{ name: "shift-log", instance: 3 }];
    readQueue(queue);

    expect(queue.push({ name: "visitor-book", instance: 4 }, { name: "ward-lantern", instance: 0 })).toBe(3);
    expect(plugin("visitor-book", "")).toBe(4);
    expect(plugin("ward-lantern", "")).toBe(0);
    expect(queue).toHaveLength(3);
  });

  test("a name provided twice before boot stops each reach with its name while another works", async () => {
    const { readQueue, plugin } = await aPage();
    readQueue([{ name: "shift-log", instance: 1 }, { name: "shift-log", instance: 2 }, { name: "ward-lantern", instance: 3 }]);

    expect(() => plugin("shift-log", "")).toThrow("More than one script provides the plugin 'shift-log'.");
    expect(() => plugin("shift-log", "")).toThrow("More than one script provides the plugin 'shift-log'.");
    expect(plugin("ward-lantern", "")).toBe(3);
  });

  test("a second provision after a first use stops every later reach and a third cannot repair it", async () => {
    const { readQueue, plugin } = await aPage();
    const queue = [{ name: "shift-log", instance: 1 }];
    readQueue(queue);
    expect(plugin("shift-log", "")).toBe(1);

    queue.push({ name: "shift-log", instance: 2 });

    expect(() => plugin("shift-log", "")).toThrow("More than one script provides the plugin 'shift-log'.");
    queue.push({ name: "shift-log", instance: 3 });
    expect(() => plugin("shift-log", "")).toThrow("More than one script provides the plugin 'shift-log'.");
  });

  test("a name not yet provided stops only until a later push provides it", async () => {
    const { readQueue, plugin } = await aPage();
    const queue: { name: string; instance: unknown }[] = [];
    readQueue(queue);
    expect(() => plugin("visitor-book", "")).toThrow("No script provides the plugin 'visitor-book'.");

    queue.push({ name: "visitor-book", instance: false });

    expect(plugin("visitor-book", "")).toBe(false);
  });

  test("the page queue keeps the core's keyed providers", async () => {
    const { readQueue, plugin, provide } = await aPage();
    provide("alis.document", () => "the document");
    provide("alis.model", (key) => key);

    readQueue([{ name: "shift-log", instance: "the log" }]);

    expect(plugin("alis.document", "")).toBe("the document");
    expect(plugin("alis.model", "admission")).toBe("admission");
    expect(plugin("shift-log", "ignored")).toBe("the log");
  });
});
