// Runtime boot waits for DOM readiness, drains queued host-page plugins, then
// boots discovered [data-reactive-plan] scripts.

import { boot, trace } from "./lifecycle/boot";
import { init as initConfirm } from "./components/fusion/confirm";
import { initNativeActionLinks } from "./components/native/native-action-link";
import "./components/native/drawer";  // side-effect: wires close button + Escape key
import "./components/native/loader";  // side-effect: handles target positioning + timeout
import { composeInitialPlans } from "./lifecycle/applied-plans";
import type { PlanDocument } from "./types/index";
import type { TraceLevel } from "./diagnostics/trace";
import { registerPlugin } from "./plugins/catalog";

interface PendingHostPlugin {
  readonly name: string;
  readonly instance: unknown;
}

interface PluginQueueWindow extends Window {
  __alisPlugins?: PendingHostPlugin[];
}

startRuntimeWhenDocumentIsReady();

function startRuntimeWhenDocumentIsReady(): void {
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", startRuntime, { once: true });
    return;
  }

  startRuntime();
}

function startRuntime(): void {
  drainPluginQueue();

  initConfirm();
  initNativeActionLinks();

  for (const activePlan of composeInitialPlans(discoverBootPlans())) {
    boot(activePlan);
  }
}

function drainPluginQueue(): void {
  const pluginQueue = window as PluginQueueWindow;
  const pendingPlugins = pluginQueue.__alisPlugins;
  if (pendingPlugins === undefined) return;

  for (const entry of pendingPlugins) {
    registerPlugin(entry.name, entry.instance);
  }
  delete pluginQueue.__alisPlugins;
}

function discoverBootPlans(): PlanDocument[] {
  const planElements = document.querySelectorAll<HTMLElement>("[data-reactive-plan]");
  const bootPlans: PlanDocument[] = [];

  for (const planElement of planElements) {
    const traceLevel = planElement.dataset.trace as TraceLevel | undefined;
    if (traceLevel) trace.setLevel(traceLevel);

    try {
      const planJson = planElement.textContent?.trim();
      if (!planJson) throw new Error("[alis] empty plan element");
      bootPlans.push(JSON.parse(planJson));
    } catch (error) {
      throw new Error(`[alis] failed to parse plan JSON from [data-reactive-plan] element: ${(error as Error).message}`);
    }
  }

  return bootPlans;
}
