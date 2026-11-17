let added = 0;

interface RoundTally {
  addOne(): number;
}

const tally: RoundTally = {
  addOne(): number {
    added += 1;
    return added;
  },
};

function provide(name: string, instance: unknown): void {
  const queue: unknown = Reflect.get(window, "__alisPlugins");
  if (Array.isArray(queue)) {
    queue.push({ name, instance });
    return;
  }
  Reflect.set(window, "__alisPlugins", [{ name, instance }]);
}

provide("round-tally", tally);
