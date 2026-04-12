// plugin-registry.ts — Plugin instance storage and resolution.
// Plugins push to window.__alisPlugins before framework boot.
// root.ts drains the queue here at module-level.

const plugins = new Map<string, unknown>();

export function registerPlugin(name: string, instance: unknown): void {
  if (!name || !name.trim()) throw new Error("[alis] plugin name must not be empty or whitespace");
  if (instance == null) throw new Error(`[alis] plugin "${name}" instance must not be null`);
  if (typeof instance !== "object") throw new Error(`[alis] plugin "${name}" must be an object`);
  if (plugins.has(name)) throw new Error(`[alis] plugin "${name}" already registered`);
  plugins.set(name, instance);
}

export function resolvePlugin(name: string): unknown {
  const instance = plugins.get(name);
  if (!instance) throw new Error(`[alis] plugin not found: "${name}"`);
  return instance;
}
