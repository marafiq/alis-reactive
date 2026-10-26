// Plugins (grammar.md 1.1): a module registered by name makes the objects `plugin(name, key, data)`
// locates. The runtime registers alis.document, alis.http, alis.model, alis.form, alis.slot,
// alis.stream and alis.hub; an app's plugins, and the framework's own components' (alis.confirm),
// arrive through window.__alisPlugins. The interpreter imports no module.

import type { PlanDocument, PluginData } from "../types/index";
import type { Listener } from "./vendor-driver";
import type { Owner } from "./owner";
import type { PlanWorld } from "./world";
import { canHaveMembers } from "../grammar/paths";

/** What a module receives with a plan's data contribution. */
export interface Contribution {
  readonly key: string;
  readonly data: PluginData;
  readonly owner: Owner;
  readonly plan: PlanDocument;
  readonly world: PlanWorld;
}

export interface Module {
  /** The object for a key: the singleton when the key is empty. */
  object(key: string): unknown;
  /** A plan's data under its owner; the module joins contributions per key and registers disposers on the owner. */
  contribute?(contribution: Contribution): void;
  /** Subscribes a handler to one of the key's events, under its owner. */
  subscribe?(key: string, event: string, listener: Listener, owner: Owner): void;
}

const modules = new Map<string, Module>();

export function registerModule(name: string, module: Module): void {
  if (modules.has(name)) throw new Error(`[alis] module "${name}" is already registered`);
  modules.set(name, module);
}

export function moduleNamed(name: string): Module {
  const module = modules.get(name);
  if (!module) throw new Error(`[alis] plugin not found: "${name}"`);
  return module;
}

/** An app plugin: an object or function the app bundle provides under a name. */
export function registerPlugin(name: string, instance: unknown): void {
  if (!canHaveMembers(instance)) throw new Error(`[alis] plugin "${name}" must be an object or function`);
  registerModule(name, { object: () => instance });
}

export function resetModulesForTests(): void {
  modules.clear();
}
