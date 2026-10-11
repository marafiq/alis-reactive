// Bindings are lexical (grammar.md 3.1): a handler binds its parameter, `let` binds a name for the
// rest of its block, and `fn` binds its parameters for its body. A name is visible where C# could
// see the variable that produced it, which the C# builder checks at render; an unbound name here is
// a plan the framework did not produce.

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
    for (let scope: Environment | undefined = this; scope; scope = scope.parent) {
      if (scope.names.has(name)) return scope.names.get(name);
    }
    throw new Error(`[alis] "${name}" is not bound here`);
  }
}
