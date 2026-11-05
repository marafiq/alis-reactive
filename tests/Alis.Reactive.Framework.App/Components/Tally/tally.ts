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
  last: null as { amount: number } | null,
  lastDay: null as number | null,
  add(amount: number): void {
    this.total += amount;
    this.last = { amount };
  },
  addEntry(entry: { amount: number; added_on: Date }): void {
    const day = entry.added_on.getDate();
    this.add(entry.amount);
    this.lastDay = day;
  },
  hasPassedTheMark(): boolean {
    return this.total > this.mark;
  },
};

const queue = window as unknown as PluginQueue;
(queue.__alisPlugins ??= []).push({ name: "tally", instance: tally });
