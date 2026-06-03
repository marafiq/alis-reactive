// Delegated handler for NativeActionLink anchors. The serialized reaction has
// one request with an empty URL; the clicked href supplies the runtime URL.

import { executeReaction } from "../../execution/reactions/execute";
import { scope } from "../../diagnostics/trace";
import type { ReactionGraph, PlanDocument, RequestPlan } from "../../types/index";
import { assertNever } from "../../shared/assert-never";

const log = scope("native-action-link");
const SELECTOR = "a[data-reactive-link]";

let initialized = false;

interface NativeActionLinkPayload {
  plan: PlanDocument;
  reaction: ReactionGraph;
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
  bindHrefToClickRequest(payload.reaction, anchor.getAttribute("href") ?? anchor.href);
  log.debug("activated", { id: anchor.id, href: anchor.href });
  const result = executeReaction(payload.reaction, payload.plan);
  if (result instanceof Promise) {
    result.catch(err => log.error("reaction.failed", { id: anchor.id, error: String(err) }));
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

function bindHrefToClickRequest(reaction: ReactionGraph, href: string): void {
  firstDeclaredRequest(reaction)!.url = href;
}

function firstDeclaredRequest(reaction: ReactionGraph): RequestPlan | undefined {
  switch (reaction.kind) {
    case "sequence":
      return firstRequestIn(reaction.steps);

    case "parallel":
      return firstRequestIn(reaction.steps)
        ?? (reaction.completion.kind === "on-settled"
          ? firstDeclaredRequest(reaction.completion.reaction)
          : undefined);

    case "branch":
      for (const branchCase of reaction.cases) {
        const request = firstDeclaredRequest(branchCase.reaction);
        if (request) return request;
      }
      return undefined;

    case "request":
      return reaction.request;

    case "set":
    case "call":
    case "dispatch":
    case "inject":
    case "show-validation-errors":
      return undefined;

    default:
      assertNever(reaction, "reaction kind");
  }
}

function firstRequestIn(reactions: readonly ReactionGraph[]): RequestPlan | undefined {
  for (const reaction of reactions) {
    const request = firstDeclaredRequest(reaction);
    if (request) return request;
  }
  return undefined;
}
