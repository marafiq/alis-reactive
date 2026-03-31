import { afterEach, describe, expect, it } from "vitest";
import { executeCommand } from "../execution/commands";
import { mutateElement } from "../execution/element";

describe("when consuming unified command values", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  it("reads a literal set-prop value from the mutation", () => {
    document.body.innerHTML = '<p id="status">old</p>';

    mutateElement({
      kind: "mutate-element",
      target: "status",
      mutation: {
        kind: "set-prop",
        prop: "textContent",
        value: { kind: "literal", value: "loaded" },
      },
    } as any);

    expect(document.getElementById("status")!.textContent).toBe("loaded");
  });

  it("applies coercion from the unified literal value", () => {
    const checkbox = document.createElement("input");
    checkbox.type = "checkbox";
    checkbox.id = "cb";
    checkbox.checked = false;
    document.body.appendChild(checkbox);

    mutateElement({
      kind: "mutate-element",
      target: "cb",
      mutation: {
        kind: "set-prop",
        prop: "checked",
        value: { kind: "literal", value: "true", coerce: "boolean" },
      },
    } as any);

    expect(checkbox.checked).toBe(true);
  });

  it("reads an event source value from the mutation itself", () => {
    const evt = { flags: { prevent: "true" } } as Record<string, unknown>;

    executeCommand({
      kind: "mutate-event",
      mutation: {
        kind: "set-prop",
        prop: "preventDefaultAction",
        value: {
          kind: "source",
          source: { kind: "event", path: "evt.flags.prevent" },
          coerce: "boolean",
        },
      },
    } as any, { evt });

    expect((evt as any).preventDefaultAction).toBe(true);
  });

  it("resolves dispatch payload fields into the final custom-event detail", () => {
    let detail: unknown = null;
    document.addEventListener("resident-saved", (e) => {
      detail = (e as CustomEvent).detail;
    });

    executeCommand({
      kind: "dispatch",
      event: "resident-saved",
      payload: {
        status: { kind: "literal", value: "ok" },
        count: { kind: "literal", value: 5 },
      },
    } as any);

    expect(detail).toEqual({ status: "ok", count: 5 });
  });
});
