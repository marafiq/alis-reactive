// A second bundle of an app that queues a plugin under a name the app's first bundle already took
// ("tally"), as two bundles that each bring their own tally would. Only a page whose layout loads it
// carries it.

interface PendingPlugin {
  readonly name: string;
  readonly instance: unknown;
}

interface PluginQueue {
  __alisPlugins?: PendingPlugin[];
}

const secondTally = {
  total: 100,
  add(amount: number): void {
    this.total += amount;
  },
};

const queue = window as unknown as PluginQueue;
(queue.__alisPlugins ??= []).push({ name: "tally", instance: secondTally });

export {};
