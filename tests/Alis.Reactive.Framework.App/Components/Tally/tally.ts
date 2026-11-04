interface PendingPlugin {
  readonly name: string;
  readonly instance: object;
}

interface PluginQueue {
  __alisPlugins?: PendingPlugin[];
}

const tally = {
  total: 0,
  mark: 10,
  add(amount: number): void {
    this.total += amount;
  },
  hasPassedTheMark(): boolean {
    return this.total > this.mark;
  },
};

const queue = window as unknown as PluginQueue;
(queue.__alisPlugins ??= []).push({ name: "tally", instance: tally });
