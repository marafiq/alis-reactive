// Plugins bridge host-page functions that are outside the Reactive Plan DSL.
// Plan still declares the callable contract; this catalog only owns the
// host-provided implementation objects.

type PluginFunction = (...args: unknown[]) => unknown;
export type PluginRoot = object | PluginFunction;

export class PluginCatalog {
  private readonly plugins = new Map<string, PluginRoot>();

  register(name: string, instance: unknown): void {
    assertPluginName(name);
    const plugin = requirePluginRoot(name, instance);
    if (this.plugins.has(name)) throw new Error(`[alis] plugin "${name}" already registered`);
    this.plugins.set(name, plugin);
  }

  resolve(name: string): PluginRoot {
    assertPluginName(name);
    const plugin = this.plugins.get(name);
    if (!plugin) throw new Error(`[alis] plugin not found: "${name}"`);
    return plugin;
  }

  clear(): void {
    this.plugins.clear();
  }
}

export const pluginCatalog = new PluginCatalog();

export function registerPlugin(name: string, instance: unknown): void {
  pluginCatalog.register(name, instance);
}

export function resolvePlugin(name: string): PluginRoot {
  return pluginCatalog.resolve(name);
}

export function resetPluginCatalogForTests(): void {
  pluginCatalog.clear();
}

function assertPluginName(name: string): void {
  const pluginNameWasProvided = name.length > 0;
  const pluginNameHasText = name.trim().length > 0;
  const pluginNameIsValid = pluginNameWasProvided && pluginNameHasText;
  if (!pluginNameIsValid) throw new Error("[alis] plugin name must not be empty or whitespace");

  const pluginNameContainsWhitespace = /\s/.test(name);
  if (pluginNameContainsWhitespace) throw new Error("[alis] plugin name must not contain whitespace");
}

function requirePluginRoot(name: string, instance: unknown): PluginRoot {
  const implementationWasProvided = instance !== null && instance !== undefined;
  if (!implementationWasProvided) {
    throw new Error(`[alis] plugin "${name}" instance must not be null`);
  }

  const implementationIsObject = typeof instance === "object";
  const implementationIsFunction = typeof instance === "function";
  const implementationCanExposeMembers = implementationIsObject || implementationIsFunction;
  if (!implementationCanExposeMembers) {
    throw new Error(`[alis] plugin "${name}" must be an object or function`);
  }

  return instance as PluginRoot;
}
