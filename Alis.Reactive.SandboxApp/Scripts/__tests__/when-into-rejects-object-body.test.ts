import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";

let executeCommand: typeof import("../execution/commands").executeCommand;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body></body></html>`);
  (globalThis as any).document = dom.window.document;
  const mod = await import("../execution/commands");
  executeCommand = mod.executeCommand;
});

describe("when Into command receives response body", () => {
  it("injects HTML when response is a string", () => {
    const container = document.createElement("div");
    container.id = "target";
    document.body.appendChild(container);

    executeCommand(
      { kind: "into", target: "target" } as any,
      { responseBody: "<p>Hello</p>" },
    );
    expect(container.innerHTML).toContain("Hello");
  });

  it("throws when response is a JSON object", () => {
    const container = document.createElement("div");
    container.id = "target";
    document.body.appendChild(container);

    expect(() => executeCommand(
      { kind: "into", target: "target" } as any,
      { responseBody: { data: "test" } },
    )).toThrow(/Into.*received object/);
  });
});
