// native-action-link.ts — Click handler for <a data-reactive-link> elements.
// Uses Plan + Reaction types for V3 plan-driven navigation.

import { executeReaction } from "../../execution/execute";
import { scope } from "../../core/trace";
import type { Reaction, Plan, Request } from "../../types";
import { RuntimeReactionTree } from "../../domain/reaction-tree";

const log = scope("native-action-link");
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

function bindHrefToSingleRequest(reaction: Reaction, href: string): void {
  NativeActionLinkRequestTarget.from(reaction).bindHref(href);
}

class NativeActionLinkRequestTarget {
  private constructor(private readonly request: Request) {}

  static from(reaction: Reaction): NativeActionLinkRequestTarget {
    const requests = RuntimeReactionTree.from(reaction).declaredRequests();
    const request = requests[0];
    if (requests.length !== 1 || request === undefined) {
      throw new Error("NativeActionLink requires exactly one request.");
    }

    return new NativeActionLinkRequestTarget(request).assertSupported();
  }

  bindHref(href: string): void {
    this.request.url = href;
  }

  private assertSupported(): NativeActionLinkRequestTarget {
    const requestUsesChaining = this.request.chain.kind === "follow-up";
    if (requestUsesChaining) {
      throw new Error("NativeActionLink does not support chained requests.");
    }

    const requestRunsClientValidation = this.request.validation.kind === "container";
    if (requestRunsClientValidation) {
      throw new Error("NativeActionLink does not support validation.");
    }

    return this;
  }
}
