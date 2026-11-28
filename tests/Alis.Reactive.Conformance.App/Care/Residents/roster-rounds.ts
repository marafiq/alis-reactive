let rounds = 0;

interface RosterRounds {
  next(): number;
}

const rosterRounds: RosterRounds = {
  next(): number {
    rounds += 1;
    return rounds;
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

provide("roster-rounds", rosterRounds);
