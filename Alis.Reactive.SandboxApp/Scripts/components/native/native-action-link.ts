// native-action-link.ts — Click handler for <a data-reactive-link> elements.
// Uses Plan + Reaction types for V3 plan-driven navigation.

import { executeReaction } from "../../execution/execute";
import { tracer } from "../../tracing";
import type { Reaction, Plan, Request } from "../../types";
import { assertNever } from "../../core/assert-never";

const t = tracer("native-action-link");
const SELECTOR = "a[data-reactive-link]";

let initialized = false;

interface NativeActionLinkPayload {
  plan: Plan;
  reaction: Reaction;
}

export function initNativeActionLinks(): void {
  if (initialized) return;
  initialized = true;
  document.addEventListener("click", handleClick);
}

export function resetNativeActionLinksForTests(): void {
  if (!initialized) return;
  document.removeEventListener("click", handleClick);
  initialized = false;
}

function handleClick(event: MouseEvent): void {
  if (event.defaultPrevented || event.button !== 0) return;
  if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;

  const target = event.target as Element | null;
  const anchor = target?.closest<HTMLAnchorElement>(SELECTOR);
  if (!anchor) return;

  event.preventDefault();

  const payload = decodePayload(anchor);
  bindHrefToSingleRequest(payload.reaction, anchor.getAttribute("href") ?? anchor.href);
  t.debug("action-link.start", { id: anchor.id, href: anchor.href });
  const result = executeReaction(payload.reaction, payload.plan);
  if (result instanceof Promise) {
    result.catch(err => t.error("action-link.fail", { id: anchor.id }, err as Error));
  }
}

function decodePayload(anchor: HTMLAnchorElement): NativeActionLinkPayload {
  const raw = anchor.dataset.reactiveLink;
  if (!raw) {
    throw new Error("NativeActionLink is missing data-reactive-link.");
  }

  try {
    return JSON.parse(raw) as NativeActionLinkPayload;
  } catch (error) {
    throw new Error(`NativeActionLink payload is invalid JSON: ${String(error)}`);
  }
}

function bindHrefToSingleRequest(reaction: Reaction, href: string): void {
  const state = { count: 0, request: undefined as Request | undefined };
  resolveSingleRequest(reaction, state);

  if (state.count !== 1 || !state.request) {
    throw new Error("NativeActionLink requires exactly one request.");
  }

  state.request.url = href;
}

function resolveSingleRequest(
  reaction: Reaction,
  state: { count: number; request?: Request },
): void {
  switch (reaction.kind) {
    case "sequence":
      for (const step of reaction.steps) resolveSingleRequest(step, state);
      return;
    case "parallel":
      for (const step of reaction.steps) resolveSingleRequest(step, state);
      return;
    case "branch":
      for (const c of reaction.cases) resolveSingleRequest(c.reaction, state);
      return;
    case "request":
      state.count++;
      if (state.count > 1) {
        throw new Error("NativeActionLink supports exactly one request.");
      }
      assertRequestSupported(reaction.request);
      state.request = reaction.request;
      return;
    case "set":
    case "call":
    case "dispatch":
    case "inject":
    case "show-validation-errors":
      return;
    default:
      assertNever(reaction, "reaction kind in NativeActionLink");
  }
}

function assertRequestSupported(request: Request): void {
  if (request.next) {
    throw new Error("NativeActionLink does not support chained requests.");
  }

  if (request.container) {
    throw new Error("NativeActionLink does not support validation.");
  }
}
