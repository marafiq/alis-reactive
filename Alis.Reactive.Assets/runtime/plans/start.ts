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

export interface OwnedPlans {
  readonly owner: Owner;
  readonly plans: readonly PlanDocument[];
}

export function startOwners(owned: readonly OwnedPlans[]): void {
  const located = owned.map(({ owner, plans }) => ({ owner, plans, objectsByPlan: plans.map(plan => new PlanObjects(plan)) }));
  for (const { owner, objectsByPlan } of located) {
    for (const objects of objectsByPlan) contribute(objects, owner);
  }
  for (const { owner, objectsByPlan } of located) {
    for (const objects of objectsByPlan) subscribeHandlers(objects, owner);
  }
  for (const { owner, plans } of located) {
    owner.start();
    log.info("owner.started", { owner: owner.name, plans: plans.map(plan => plan.planId) });
  }
}

export function startOwner(owner: Owner, plans: readonly PlanDocument[]): void {
  startOwners([{ owner, plans }]);
}

function contribute(objects: PlanObjects, owner: Owner): void {
  for (const locator of Object.values(objects.plan.objects)) {
    if (locator.kind !== "plugin" || locator.data.kind === "none") continue;
    pluginNamed(locator.name).contribute!({ key: locator.key, data: locator.data, owner, plan: objects.plan, objects });
  }
}

function subscribeHandlers(objects: PlanObjects, owner: Owner): void {
  for (const handler of objects.plan.handlers) {
    try {
      objects.subscribe(handler.on, handler.path, handler.event, payload => runHandler(handler, payload, objects), owner);
    } catch (error) {
      log.error("handler.subscribe-failed", { error: toJavaScriptString(error), on: handler.on, event: handler.event });
    }
  }
}

const PAYLOAD = "e";

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
