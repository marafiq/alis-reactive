// Starting an owner (grammar.md 3.5): locate each plan's objects, hand every plugin its data
// contribution under the owner, subscribe every handler, then start the owner, which runs the
// handlers waiting for this owner's start (the document's `ready`). A slot's load starts only the
// slot's owner, so a ready handler that loads a slot does not run again.

import type { Handler, PlanDocument } from "../types/index";
import { Environment } from "../grammar/environment";
import { isPromise } from "../grammar/evaluate";
import { executeBlock } from "../grammar/execute";
import { pluginNamed } from "../objects/plugins";
import type { Owner } from "../objects/owner";
import { PlanObjects } from "../objects/plan-objects";
import { logger } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = logger("plans");

export function startOwner(owner: Owner, plans: readonly PlanDocument[]): void {
  const objectsByPlan = plans.map(plan => new PlanObjects(plan));
  for (const objects of objectsByPlan) contribute(objects, owner);
  for (const objects of objectsByPlan) subscribeHandlers(objects, owner);
  owner.start();
  log.info("owner.started", { owner: owner.name, plans: plans.map(plan => plan.planId) });
}

// The plan gives data only to alis.model and alis.form, which the runtime registers, each with contribute.
function contribute(objects: PlanObjects, owner: Owner): void {
  for (const locator of Object.values(objects.plan.objects)) {
    if (locator.kind !== "plugin" || locator.data.kind === "none") continue;
    pluginNamed(locator.name).contribute!({ key: locator.key, data: locator.data, owner, plan: objects.plan, objects });
  }
}

// A handler whose object is not constructed yet fails at the boundary when it subscribes; it stops
// no other handler (grammar.md 3.4).
function subscribeHandlers(objects: PlanObjects, owner: Owner): void {
  for (const handler of objects.plan.handlers) {
    try {
      objects.subscribe(handler.on, handler.path, handler.event, payload => runHandler(handler, payload, objects), owner);
    } catch (error) {
      log.error("handler.subscribe-failed", { error: toJavaScriptString(error), on: handler.on, event: handler.event });
    }
  }
}

// A handler's block reads its payload by this name (grammar.md 1.1).
const PAYLOAD = "e";

/** Runs one handler. A failing handler is logged and stops no other handler (grammar.md 3.4). */
export function runHandler(handler: Handler, payload: unknown, objects: PlanObjects): void {
  const env = Environment.root();
  env.bind(PAYLOAD, payload);
  const source = `${handler.on}:${handler.event}`;
  try {
    const completion = executeBlock(handler.do, { env, objects });
    if (isPromise(completion)) {
      completion.catch(error => log.error("handler.failed", { error: toJavaScriptString(error), source }));
    }
  } catch (error) {
    log.error("handler.failed", { error: toJavaScriptString(error), source });
  }
}
