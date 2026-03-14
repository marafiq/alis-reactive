import { enrichReaction, getBootedPlan } from "./boot";
import { executeHttpReaction } from "./pipeline";
import { scope } from "./trace";
import type {
  ComponentEntry,
  HttpReaction,
  Reaction,
  RequestDescriptor,
  StatusHandler,
  ValidationDescriptor,
} from "./types";

const log = scope("native-action-link");
const SELECTOR = "a[data-reactive-link]";

let initialized = false;

interface NativeActionLinkPayload {
  planId: string;
  reaction: HttpReaction;
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

  const payload = decodePayload(anchor);
  assertSupportedReaction(payload.reaction);

  const components = resolveComponents(payload.planId);
  enrichReaction(payload.reaction, components);

  if (requiresPlanContext(payload.reaction.request) && Object.keys(components).length === 0) {
    throw new Error(
      `NativeActionLink requires a booted plan context for planId '${payload.planId}' ` +
      "when using IncludeAll() or unresolved validation fields."
    );
  }

  assertValidationIsEnriched(payload.reaction.request.validation);

  event.preventDefault();
  log.debug("activate", { id: anchor.id, href: anchor.href, planId: payload.planId });
  void executeHttpReaction(payload.reaction, { components });
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

function resolveComponents(planId: string): Record<string, ComponentEntry> {
  return getBootedPlan(planId)?.components ?? {};
}

function requiresPlanContext(request: RequestDescriptor): boolean {
  if (request.gather?.some(item => item.kind === "all")) {
    return true;
  }

  return hasIncompleteValidation(request.validation);
}

function hasIncompleteValidation(desc?: ValidationDescriptor): boolean {
  if (!desc) return false;
  return desc.fields.some(field => !field.fieldId || !field.vendor || !field.readExpr);
}

function assertValidationIsEnriched(desc?: ValidationDescriptor): void {
  if (!desc) return;

  const invalidField = desc.fields.find(field => !field.fieldId || !field.vendor || !field.readExpr);
  if (invalidField) {
    throw new Error(
      `NativeActionLink validation field '${invalidField.fieldName}' is missing component metadata.`
    );
  }
}

function assertSupportedReaction(reaction: Reaction): void {
  if (reaction.kind !== "http") {
    throw new Error("NativeActionLink supports exactly one HTTP reaction.");
  }

  if (reaction.request.chained) {
    throw new Error("NativeActionLink does not support chained requests.");
  }

  assertHandlersContainNoNestedHttp(reaction.request.onSuccess);
  assertHandlersContainNoNestedHttp(reaction.request.onError);
}

function assertHandlersContainNoNestedHttp(handlers?: StatusHandler[]): void {
  if (!handlers) return;
  for (const handler of handlers) {
    if (handler.reaction) {
      assertReactionContainsNoHttp(handler.reaction);
    }
  }
}

function assertReactionContainsNoHttp(reaction: Reaction): void {
  switch (reaction.kind) {
    case "sequential":
      return;
    case "conditional":
      for (const branch of reaction.branches) {
        assertReactionContainsNoHttp(branch.reaction);
      }
      return;
    case "http":
    case "parallel-http":
      throw new Error("NativeActionLink response handlers cannot start nested HTTP requests.");
  }
}
