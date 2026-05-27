import { afterEach, describe, expect, it, vi } from "vitest";
import {
  initNativeActionLinks,
  resetNativeActionLinksForTests,
} from "../components/native/native-action-link";
import type { PlanDocument, ReactionGraph, RequestPlan } from "../types";

const emptyPlan: PlanDocument = {
  version: 3,
  planId: "Runtime.NativeActionLink",
  scope: { kind: "root" },
  types: {},
  components: {},
  behaviors: [],
};

afterEach(() => {
  vi.unstubAllGlobals();
  resetNativeActionLinksForTests();
  document.body.innerHTML = "";
});

function request(url: string, overrides: Partial<RequestPlan> = {}): RequestPlan {
  return {
    method: "DELETE",
    url,
    validation: { kind: "none" },
    input: { kind: "none" },
    before: [],
    success: [],
    error: [],
    complete: [],
    chain: { kind: "terminal" },
    ...overrides,
  };
}

function requestReaction(url: string, overrides: Partial<RequestPlan> = {}): ReactionGraph {
  return {
    kind: "request",
    request: request(url, overrides),
  };
}

function renderActionLink(reaction: ReactionGraph, href = "/residents/42/delete"): HTMLAnchorElement {
  const anchor = document.createElement("a");
  anchor.id = "delete-resident";
  anchor.href = href;
  anchor.dataset.reactiveLink = JSON.stringify({ plan: emptyPlan, reaction });
  document.body.appendChild(anchor);
  return anchor;
}

async function nextTask(): Promise<void> {
  await new Promise(resolve => setTimeout(resolve, 0));
}

describe("native action link", () => {
  it("binds the clicked href to the single serialized request", async () => {
    const fetchMock = vi.fn(async () => new Response("", { status: 204 }));
    vi.stubGlobal("fetch", fetchMock);

    const anchor = renderActionLink(requestReaction("/placeholder"));
    initNativeActionLinks();

    anchor.click();
    await nextTask();

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("/residents/42/delete");
    expect(init.method).toBe("DELETE");
  });
});
