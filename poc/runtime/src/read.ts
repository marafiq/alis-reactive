export type Read =
  | { readonly kind: "value"; readonly value: unknown }
  | { readonly kind: "nothing" };

export const nothing: Read = { kind: "nothing" };

export function value(held: unknown): Read {
  return { kind: "value", value: held };
}

export class Stop extends Error {
  constructor(readonly reason: string) {
    super(reason);
    this.name = "Stop";
  }
}

export function assertNever(unexpected: never): never {
  throw new Error("The plan holds a node this runtime does not know: " + JSON.stringify(unexpected));
}

export function heldOrNull(read: Read): unknown {
  return read.kind === "value" ? read.value : null;
}
