import { describe, expect, it } from "vitest";
import { RuntimeReactionTree } from "../domain/reaction-tree";
import type { Reaction, Request } from "../types";

function request(url: string, overrides: Partial<Request> = {}): Request {
  return {
    method: "GET",
    url,
    headers: {},
    routeParams: {},
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

function requestReaction(url: string, overrides: Partial<Request> = {}): Reaction {
  return {
    kind: "request",
    request: request(url, overrides),
  };
}

describe("RuntimeReactionTree", () => {
  it("walks declared requests through sequence, branch, and parallel completion", () => {
    const reaction: Reaction = {
      kind: "sequence",
      steps: [
        requestReaction("/before"),
        {
          kind: "branch",
          cases: [
            {
              guard: { kind: "default" },
              reaction: requestReaction("/branch"),
            },
          ],
        },
        {
          kind: "parallel",
          steps: [requestReaction("/parallel")],
          completion: {
            kind: "on-settled",
            reaction: requestReaction("/parallel-complete"),
          },
        },
      ],
    };

    expect(RuntimeReactionTree.from(reaction).declaredRequests().map(req => req.url))
      .toEqual(["/before", "/branch", "/parallel", "/parallel-complete"]);
  });

  it("does not treat follow-up requests as declared reaction nodes", () => {
    const reaction = requestReaction("/first", {
      chain: {
        kind: "follow-up",
        next: request("/next"),
      },
    });

    expect(RuntimeReactionTree.from(reaction).declaredRequests().map(req => req.url))
      .toEqual(["/first"]);
  });
});
