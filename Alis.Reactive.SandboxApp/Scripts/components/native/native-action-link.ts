import { executeAction } from "../../execution/execute";
import { scope } from "../../core/trace";
import type { Plan, PlanAction, RequestPlan, ResponseHandlerPlan } from "../../types";

const log = scope("native-action-link");
const SELECTOR = "a[data-reactive-link]";

let initialized = false;

interface NativeActionLinkPayload {
  plan: Plan;
  action: PlanAction;
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
  bindHrefToSingleRequest(payload.action, anchor.getAttribute("href") ?? anchor.href);
  log.debug("activate", { id: anchor.id, href: anchor.href });
  executeAction(payload.action, { plan: payload.plan }).catch(error =>
    log.error("action failed", { error: String(error) })
  );
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

function bindHrefToSingleRequest(action: PlanAction, href: string): void {
  const state = { count: 0, request: undefined as RequestPlan | undefined };
  resolveSingleRequest(action, state);

  if (state.count !== 1 || !state.request) {
    throw new Error("NativeActionLink requires exactly one request.");
  }

  state.request.url = href;
}

function resolveSingleRequest(action: PlanAction, state: { count: number; request?: RequestPlan }): void {
  switch (action.kind) {
    case "sequence":
      for (const step of action.steps) resolveSingleRequest(step, state);
      return;

    case "branch":
      for (const branch of action.cases) resolveSingleRequest(branch.run, state);
      return;

    case "request":
      state.count++;
      if (state.count > 1) {
        throw new Error("NativeActionLink supports exactly one request.");
      }
      assertRequestSupported(action.request);
      state.request = action.request;
      return;

    case "parallel":
      throw new Error("NativeActionLink does not support Parallel().");

    default:
      return;
  }
}

function assertRequestSupported(request: RequestPlan): void {
  if (request.next) {
    throw new Error("NativeActionLink does not support chained requests.");
  }

  if (request.validation) {
    throw new Error("NativeActionLink does not support validation.");
  }

  if (request.input?.value.kind === "binding-map" && request.input.value.include === "all") {
    throw new Error("NativeActionLink does not support IncludeAll(). Use explicit gather instead.");
  }

  assertActionContainsNoRequestList(request.before);
  assertHandlersContainNoRequest(request.onSuccess);
  assertHandlersContainNoRequest(request.onError);
  assertActionContainsNoRequestList(request.onSettled);
}

function assertHandlersContainNoRequest(handlers?: ResponseHandlerPlan[]): void {
  if (!handlers) return;
  for (const handler of handlers) {
    assertNestedActionContainsNoRequest(handler.run);
  }
}

function assertActionContainsNoRequestList(actions?: PlanAction[]): void {
  if (!actions) return;
  for (const action of actions) {
    assertNestedActionContainsNoRequest(action);
  }
}

function assertNestedActionContainsNoRequest(action: PlanAction): void {
  switch (action.kind) {
    case "sequence":
      for (const step of action.steps) assertNestedActionContainsNoRequest(step);
      return;

    case "branch":
      for (const branch of action.cases) assertNestedActionContainsNoRequest(branch.run);
      return;

    case "request":
    case "parallel":
      throw new Error("NativeActionLink response handlers cannot start a second HTTP request.");

    default:
      return;
  }
}
