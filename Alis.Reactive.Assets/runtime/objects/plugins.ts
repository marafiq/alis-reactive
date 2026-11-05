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
