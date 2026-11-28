let added = 0;

interface CardTally {
  addOne(): number;
  readonly count: number;
}

const tally: CardTally = {
  addOne(): number {
    added += 1;
    return added;
  },
  get count(): number {
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

provide("card-tally", tally);
