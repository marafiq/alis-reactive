import { beforeEach, describe, expect, it, vi } from "vitest";
import { initNativeActionLinks } from "../components/native/native-action-link";
import type { PlanAction } from "../types";
import { createPlan, flushAsync, htmlBlockContract } from "./support/v2-fixtures";

function jsonResponse(body: unknown) {
  return {
    ok: true,
    status: 200,
    headers: {
      get(name: string) {
        return name.toLowerCase() === "content-type" ? "application/json" : null;
      },
    },
    json: () => Promise.resolve(body),
    text: () => Promise.resolve(JSON.stringify(body)),
  };
}

describe("when following a native action link", () => {
  beforeEach(() => {
    document.body.innerHTML = '<div id="status"></div>';
  });

  it("binds the anchor href into the single request and executes it", async () => {
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse({ message: "loaded" }));
    (globalThis as { fetch?: unknown }).fetch = fetchMock;

    const plan = createPlan({
      contracts: {
        "html.block": htmlBlockContract,
      },
      objects: {
        status: { contract: "html.block", elementId: "status" },
      },
    });

    const action: PlanAction = {
      kind: "request",
      request: {
        method: "GET",
        url: "/placeholder",
        onSuccess: [
          {
            run: {
              kind: "set",
              target: { object: "status", member: "text" },
              value: { kind: "context", scope: "response", path: [{ prop: "message" }] },
            },
          },
        ],
      },
    };

    const anchor = document.createElement("a");
    anchor.id = "details";
    anchor.textContent = "Open";
    anchor.setAttribute("href", "/residents/42");
    anchor.dataset.reactiveLink = JSON.stringify({ plan, action });
    document.body.appendChild(anchor);

    initNativeActionLinks();
    anchor.click();
    await flushAsync();
    await flushAsync();

    expect(fetchMock).toHaveBeenCalledWith("/residents/42", expect.objectContaining({ method: "GET" }));
    expect(document.getElementById("status")?.textContent).toBe("loaded");
  });
});
