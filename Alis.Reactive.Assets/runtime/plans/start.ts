// Starting an owner (grammar.md 3.5): locate each plan's objects, hand every plugin its data
// contribution under the owner, subscribe every handler, then start the owner, which runs the
// handlers waiting for this owner's start (the document's `ready`). A slot's load starts only the
// slot's owner, so a ready handler that loads a slot does not run again.

import type { Handler, PlanDocument } from "../types/index";
import { Environment } from "../grammar/environment";
import { isPromise } from "../grammar/evaluate";
import { executeBlock } from "../grammar/execute";
import { moduleNamed } from "../objects/modules";
import type { Owner } from "../objects/owner";
import { PlanWorld } from "../objects/world";
import { scope } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = scope("plans");

export function startOwner(owner: Owner, plans: readonly PlanDocument[]): void {
  const worlds = plans.map(plan => new PlanWorld(plan));
  for (const world of worlds) contribute(world, owner);
  for (const world of worlds) subscribeHandlers(world, owner);
  owner.start();
  log.info("owner.started", { owner: owner.name, plans: plans.map(plan => plan.planId) });
}

function contribute(world: PlanWorld, owner: Owner): void {
  for (const [id, locator] of Object.entries(world.plan.objects)) {
    if (locator.kind !== "plugin" || locator.data.kind === "none") continue;
    const module = moduleNamed(locator.name);
    if (!module.contribute) throw new Error(`[alis] plugin "${locator.name}" takes no data (object "${id}")`);
    module.contribute({ key: locator.key, data: locator.data, owner, plan: world.plan, world });
  }
}

function subscribeHandlers(world: PlanWorld, owner: Owner): void {
  for (const handler of world.plan.handlers) {
    try {
      world.subscribe(handler.on, handler.path, handler.event, payload => runHandler(handler, payload, world), owner);
    } catch (error) {
      log.error("handler.subscribe-failed", { error: toJavaScriptString(error), on: handler.on, event: handler.event });
    }
  }
}

/** Runs one handler. A failing handler is logged and stops no other handler (grammar.md 3.4). */
export function runHandler(handler: Handler, payload: unknown, world: PlanWorld): void {
  const env = Environment.root();
  env.bind(handler.param, payload);
  const source = `${handler.on}:${handler.event}`;
  try {
    const completion = executeBlock(handler.do, { env, world });
    if (isPromise(completion)) {
      completion.catch(error => log.error("handler.failed", { error: toJavaScriptString(error), source }));
    }
  } catch (error) {
    log.error("handler.failed", { error: toJavaScriptString(error), source });
  }
}
