// Plugins (grammar.md 1.1): a plugin registered by name makes the objects `plugin(name, key, data)`
// locates. The runtime registers alis.document, alis.http, alis.model, alis.form, alis.slot,
// alis.stream and alis.hub; an app's plugins, and the framework's own components' (alis.confirm),
// arrive through window.__alisPlugins. The interpreter imports no plugin.

import type { PlanDocument, PluginData } from "../types/index";
import type { Listener } from "./vendor-driver";
import type { Owner } from "./owner";
import type { PlanObjects } from "./plan-objects";
import { canHaveMembers } from "../grammar/paths";

export interface Contribution {
  readonly key: string;
  readonly data: PluginData;
  readonly owner: Owner;
  readonly plan: PlanDocument;
  readonly objects: PlanObjects;
}

export interface Plugin {
  object(key: string, data: PluginData): unknown;
  contribute?(contribution: Contribution): void;
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

export function pluginOf(name: string, instance: unknown): Plugin {
  if (!canHaveMembers(instance)) throw new Error(`[alis] plugin "${name}" must be an object or function`);
  return { object: () => instance };
}
