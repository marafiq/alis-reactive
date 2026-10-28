// A second bundle of an app that queues a plugin whose instance can hold no members (a number where an
// object belongs), as a bundle that queues the wrong value would. Only a page whose layout loads it
// carries it.

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
