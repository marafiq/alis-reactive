import { executeReaction } from "../../execution/execute";
import { scope } from "../../core/trace";
import type {
  Reaction,
  RequestDescriptor,
  StatusHandler,
} from "../../types";
import { assertNever } from "../../core/assert-never";

const log = scope("native-action-link");
const SELECTOR = "a[data-reactive-link]";

let initialized = false;

interface NativeActionLinkPayload {
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
  log.debug("activate", { id: anchor.id, href: anchor.href });
  executeReaction(payload.reaction).catch(err =>
    log.error("reaction failed", { error: String(err) })
  );
}

function decodePayload(anchor: HTMLAnchorElement): NativeActionLinkPayload {
  const raw = anchor.getAttribute("data-reactive-link");
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
  const state = { count: 0, request: undefined as RequestDescriptor | undefined };
  resolveSingleRequest(reaction, state);

  if (state.count !== 1 || !state.request) {
    throw new Error("NativeActionLink requires exactly one request.");
  }

  state.request.url = href;
}

function resolveSingleRequest(
  reaction: Reaction,
  state: { count: number; request?: RequestDescriptor }
): void {
  switch (reaction.kind) {
    case "sequential":
      return;
    case "conditional":
      for (const branch of reaction.branches) {
        resolveSingleRequest(branch.reaction, state);
      }
      return;
    case "http":
      state.count++;
      if (state.count > 1) {
        throw new Error("NativeActionLink supports exactly one request.");
      }
      assertRequestSupported(reaction.request);
      state.request = reaction.request;
      return;
    case "parallel-http":
      throw new Error("NativeActionLink does not support Parallel().");
    default:
      assertNever(reaction, "reaction kind in NativeActionLink");
  }
}

function assertRequestSupported(request: RequestDescriptor): void {
  if (request.chained) {
    throw new Error("NativeActionLink does not support chained requests.");
  }

  if (request.validation) {
    throw new Error("NativeActionLink does not support validation.");
  }

  if (request.gather?.some(item => item.kind === "all")) {
    throw new Error("NativeActionLink does not support IncludeAll(). Use explicit gather instead.");
  }

  assertHandlersContainNoRequest(request.onSuccess);
  assertHandlersContainNoRequest(request.onError);
}

function assertHandlersContainNoRequest(handlers?: StatusHandler[]): void {
  if (!handlers) return;
  for (const handler of handlers) {
    if (handler.reaction) {
      assertNestedReactionContainsNoRequest(handler.reaction);
    }
  }
}

function assertNestedReactionContainsNoRequest(reaction: Reaction): void {
  switch (reaction.kind) {
    case "sequential":
      return;
    case "conditional":
      for (const branch of reaction.branches) {
        assertNestedReactionContainsNoRequest(branch.reaction);
      }
      return;
    case "http":
    case "parallel-http":
      throw new Error("NativeActionLink response handlers cannot start a second HTTP request.");
    default:
      assertNever(reaction, "reaction kind in NativeActionLink handler");
  }
}
