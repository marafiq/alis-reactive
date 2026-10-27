// Plugins (grammar.md 1.1): a plugin registered by name makes the objects `plugin(name, key, data)`
// locates. The runtime registers alis.document, alis.http, alis.model, alis.form, alis.slot,
// alis.stream and alis.hub; an app's plugins, and the framework's own components' (alis.confirm),
// arrive through window.__alisPlugins. The interpreter imports no plugin.

import type { PlanDocument, PluginData } from "../types/index";
import type { Listener } from "./vendor-driver";
import type { Owner } from "./owner";
import type { PlanObjects } from "./plan-objects";
import { canHaveMembers } from "../grammar/paths";

/** What a plugin receives with a plan's data contribution. */
export interface Contribution {
  readonly key: string;
  readonly data: PluginData;
  readonly owner: Owner;
  readonly plan: PlanDocument;
  readonly objects: PlanObjects;
}

export interface Plugin {
  /** The object for a key: the singleton when the key is empty. */
  object(key: string): unknown;
  /** A plan's data under its owner; the plugin joins contributions per key and registers disposers on the owner. */
  contribute?(contribution: Contribution): void;
  /** Subscribes a handler to one of the key's events, under its owner. */
  subscribe?(key: string, event: string, listener: Listener, owner: Owner): void;
}

const plugins = new Map<string, Plugin>();

export function registerPlugin(name: string, plugin: Plugin): void {
  if (plugins.has(name)) throw new Error(`[alis] plugin "${name}" is already registered`);
  plugins.set(name, plugin);
}

export function pluginNamed(name: string): Plugin {
  const plugin = plugins.get(name);
  if (!plugin) throw new Error(`[alis] plugin not found: "${name}"`);
  return plugin;
}

/** An app's plugin: one object or function the bundle provides under a name, with no keys and no data. */
export function pluginOf(name: string, instance: unknown): Plugin {
  if (!canHaveMembers(instance)) throw new Error(`[alis] plugin "${name}" must be an object or function`);
  return { object: () => instance };
}
