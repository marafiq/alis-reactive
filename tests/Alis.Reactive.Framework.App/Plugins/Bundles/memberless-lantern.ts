interface PendingPlugin {
  readonly name: string;
  readonly instance: unknown;
}

interface PluginQueue {
  __alisPlugins?: PendingPlugin[];
}

const queue = window as unknown as PluginQueue;
(queue.__alisPlugins ??= []).push({ name: "lantern", instance: 42 });

export {};
