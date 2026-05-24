// root.ts — ESM entry point for alis-reactive runtime
// esbuild bundles from here. Auto-discovers [data-reactive-plan] elements on page load.
// V3: plans have version, planId, types, components, behaviors.

import { boot, trace } from "./lifecycle/boot";
import { init as initConfirm } from "./components/fusion/confirm";
import { initNativeActionLinks } from "./components/native/native-action-link";
import "./components/native/drawer";  // side-effect: wires close button + Escape key
import "./components/native/loader";  // side-effect: handles target positioning + timeout
import { composeInitialPlans } from "./lifecycle/merge-plan";
import type { Plan } from "./types";
import type { TraceLevel } from "./core/trace";
import { registerPlugin } from "./core/plugin-registry";

interface PendingBrowserPlugin {
  readonly name: string;
  readonly instance: unknown;
}

interface PluginQueueWindow extends Window {
  __alisPlugins?: PendingBrowserPlugin[];
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

  for (const plan of composeInitialPlans(discoverPlans())) {
    boot(plan);
  }
}

function drainPluginQueue(): void {
  // Plugins push here from separate bundles before the framework starts.
  const pluginQueue = window as PluginQueueWindow;
  const pendingPlugins = pluginQueue.__alisPlugins;
  if (pendingPlugins === undefined) return;

  for (const entry of pendingPlugins) {
    registerPlugin(entry.name, entry.instance);
  }
  delete pluginQueue.__alisPlugins;
}

function discoverPlans(): Plan[] {
  const planEls = document.querySelectorAll<HTMLElement>("[data-reactive-plan]");
  const plans: Plan[] = [];

  for (const el of planEls) {
    const traceLevel = el.dataset.trace as TraceLevel | undefined;
    if (traceLevel) trace.setLevel(traceLevel);

    try {
      const text = el.textContent?.trim();
      if (!text) throw new Error("[alis] empty plan element");
      plans.push(JSON.parse(text));
    } catch (e) {
      throw new Error(`[alis] failed to parse plan JSON from [data-reactive-plan] element: ${(e as Error).message}`);
    }
  }

  return plans;
}
