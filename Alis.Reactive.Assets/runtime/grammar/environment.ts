export class Environment {
  private readonly names = new Map<string, unknown>();

  private constructor(private readonly parent: Environment | undefined) {}

  static root(): Environment {
    return new Environment(undefined);
  }

  child(): Environment {
    return new Environment(this);
  }

  bind(name: string, value: unknown): void {
    this.names.set(name, value);
  }

  lookup(name: string): unknown {
    if (this.names.has(name)) return this.names.get(name);
    if (this.parent) return this.parent.lookup(name);
    throw new Error(`[alis] "${name}" is not bound here`);
  }
}
