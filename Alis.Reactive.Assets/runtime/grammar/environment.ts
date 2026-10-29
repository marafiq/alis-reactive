// Bindings are lexical (grammar.md 3.1): a handler binds its parameter, `let` binds a name for the
// rest of its block, and `fn` binds its parameters for its body. A name is visible where C# could
// see the variable that produced it, which the C# builder checks at render, and the builder reads a
// name only in the block that binds it or a block inside it: a name this block does not hold, a
// parent does.

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
    return this.names.has(name) ? this.names.get(name) : this.parent!.lookup(name);
  }
}
