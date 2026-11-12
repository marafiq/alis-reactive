export type Read = { readonly kind: "value"; readonly value: unknown } | { readonly kind: "nothing" };

export const nothing: Read = { kind: "nothing" };

export function value(held: unknown): Read {
  return held === undefined ? nothing : { kind: "value", value: held };
}

export function heldOrNull(read: Read): unknown {
  return read.kind === "value" ? read.value : null;
}

export class Stop extends Error {
  readonly reason: string;

  constructor(reason: string) {
    super(reason);
    this.name = "Stop";
    this.reason = reason;
  }
}
