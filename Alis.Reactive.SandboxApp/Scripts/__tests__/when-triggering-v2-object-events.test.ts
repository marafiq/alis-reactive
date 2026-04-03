import { beforeEach, describe, expect, it, vi } from "vitest";
import { boot } from "../lifecycle/boot";
import {
  arrayOf,
  createPlan,
  flushAsync,
  htmlBlockContract,
  method,
  property,
  scalar,
} from "./support/v2-fixtures";

describe("when triggering v2 object events", () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <div id="search"></div>
      <div id="status"></div>
    `;
  });

  it("exposes the event object as a normal capability target", async () => {
    const plan = createPlan({
      contracts: {
        "html.block": htmlBlockContract,
        "test.search": {
          kind: "component",
          resolver: "native-element",
          members: {},
          events: {
            filtering: {
              channel: "filtering",
              eventObject: { contract: "test.filtering-event" },
              data: {
                term: { kind: "member", object: "$eventObject", member: "text" },
              },
            },
          },
        },
        "test.filtering-event": {
          kind: "event-object",
          resolver: "event-object",
          members: {
            text: property("text", scalar("string"), "read"),
            preventDefaultAction: property("preventDefaultAction", scalar("boolean"), "write"),
            updateData: method("updateData", [arrayOf(scalar("raw"))]),
          },
        },
      },
      objects: {
        searchBox: { contract: "test.search", elementId: "search" },
        status: { contract: "html.block", elementId: "status" },
      },
      workflows: [
        {
          when: { kind: "object-event", object: "searchBox", event: "filtering" },
          run: {
            kind: "sequence",
            steps: [
              {
                kind: "set",
                target: { object: "$eventObject", member: "preventDefaultAction" },
                value: { kind: "literal", value: true },
              },
              {
                kind: "call",
                target: { object: "$eventObject", member: "updateData" },
                args: [
                  {
                    kind: "array",
                    items: [{ kind: "literal", value: "Oak" }],
                  },
                ],
              },
              {
                kind: "set",
                target: { object: "status", member: "text" },
                value: { kind: "context", scope: "event", path: [{ prop: "term" }] },
              },
            ],
          },
        },
      ],
    });

    boot(plan);

    const updateData = vi.fn();
    const event = new Event("filtering");
    Object.assign(event, {
      text: "Maple",
      preventDefaultAction: false,
      updateData,
    });

    document.getElementById("search")?.dispatchEvent(event);
    await flushAsync();

    expect((event as any).preventDefaultAction).toBe(true);
    expect(updateData).toHaveBeenCalledWith(["Oak"]);
    expect(document.getElementById("status")?.textContent).toBe("Maple");
  });
});
