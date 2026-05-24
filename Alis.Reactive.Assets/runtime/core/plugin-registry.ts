// plugin-registry.ts — Browser plugin instance storage.
// Plugins are an explicit bridge for browser functions that are outside the
// deterministic plan DSL. The plan still declares the callable contract; this
// registry only owns the browser-provided implementation objects.

type BrowserPluginFunction = (...args: unknown[]) => unknown;
export type BrowserPluginRoot = object | BrowserPluginFunction;

export class BrowserPluginCatalog {
  private readonly plugins = new Map<string, BrowserPluginInstance>();

  register(name: string, instance: unknown): void {
    const pluginName = BrowserPluginName.from(name);
    const pluginInstance = BrowserPluginInstance.from(pluginName, instance);
    if (this.plugins.has(pluginName.value)) throw new Error(`[alis] plugin "${pluginName.value}" already registered`);
    this.plugins.set(pluginName.value, pluginInstance);
  }

  resolve(name: string): BrowserPluginRoot {
    const pluginName = BrowserPluginName.from(name);
    const instance = this.plugins.get(pluginName.value);
    if (!instance) throw new Error(`[alis] plugin not found: "${pluginName.value}"`);
    return instance.root;
  }

  clear(): void {
    this.plugins.clear();
  }
}

export const browserPlugins = new BrowserPluginCatalog();

export function registerPlugin(name: string, instance: unknown): void {
  browserPlugins.register(name, instance);
}

export function resolvePlugin(name: string): BrowserPluginRoot {
  return browserPlugins.resolve(name);
}

export function resetPluginRegistryForTests(): void {
  browserPlugins.clear();
}

class BrowserPluginName {
  private constructor(readonly value: string) {}

  static from(name: unknown): BrowserPluginName {
    const pluginNameIsString = typeof name === "string";
    if (!pluginNameIsString) throw new Error("[alis] plugin name must be a string");

    const pluginNameWasProvided = name.length > 0;
    const pluginNameHasText = name.trim().length > 0;
    const pluginNameIsValid = pluginNameWasProvided && pluginNameHasText;
    if (!pluginNameIsValid) throw new Error("[alis] plugin name must not be empty or whitespace");

    const pluginNameContainsWhitespace = /\s/.test(name);
    if (pluginNameContainsWhitespace) throw new Error("[alis] plugin name must not contain whitespace");

    return new BrowserPluginName(name);
  }
}

class BrowserPluginInstance {
  private constructor(readonly root: BrowserPluginRoot) {}

  static from(name: BrowserPluginName, instance: unknown): BrowserPluginInstance {
    const implementationWasProvided = instance !== null && instance !== undefined;
    if (!implementationWasProvided) {
      throw new Error(`[alis] plugin "${name.value}" instance must not be null`);
    }

    const implementationIsObject = typeof instance === "object";
    const implementationIsFunction = typeof instance === "function";
    const implementationCanExposeMembers = implementationIsObject || implementationIsFunction;
    if (!implementationCanExposeMembers) {
      throw new Error(`[alis] plugin "${name.value}" must be an object or function`);
    }

    return new BrowserPluginInstance(instance as BrowserPluginRoot);
  }
}
