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
