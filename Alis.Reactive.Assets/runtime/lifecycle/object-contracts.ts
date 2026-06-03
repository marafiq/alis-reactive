// object-contracts.ts — merges browser object property, method, and event contracts.

import type { BrowserObjectContract, Shape } from "../types/index";

function cloneObjectContract(type: BrowserObjectContract): BrowserObjectContract {
  return mergeObjectContracts(emptyObjectContract(), type);
}

function emptyObjectContract(): BrowserObjectContract {
  return {
    properties: {},
    methods: {},
    events: {},
  };
}

export function mergeObjectContracts(existing: BrowserObjectContract | undefined, incoming: BrowserObjectContract): BrowserObjectContract {
  if (existing === undefined) return cloneObjectContract(incoming);

  return {
    properties: mergeMemberContracts(existing.properties, incoming.properties, mergeProperties),
    methods: mergeMemberContracts(existing.methods, incoming.methods, mergeMethods),
    events: mergeMemberContracts(existing.events, incoming.events, mergeEvents),
  };
}

function mergeMemberContracts<T>(
  existing: Record<string, T>,
  incoming: Record<string, T>,
  merge: (left: T, right: T) => T,
): Record<string, T> {
  const merged = { ...existing };
  for (const [member, incomingContract] of Object.entries(incoming)) {
    const existingContract = merged[member];
    merged[member] = existingContract === undefined
      ? incomingContract
      : merge(existingContract, incomingContract);
  }

  return merged;
}

type ContractProperty = BrowserObjectContract["properties"][string];
type ContractMethod = BrowserObjectContract["methods"][string];
type ContractEvent = BrowserObjectContract["events"][string];

function mergeProperties(existing: ContractProperty, incoming: ContractProperty): ContractProperty {
  return {
    ...incoming,
    shape: mergeObjectContractShapes(existing.shape, incoming.shape),
    access: mergeMemberAccess(existing.access, incoming.access),
  };
}

function mergeMemberAccess(
  existing: ContractProperty["access"],
  incoming: ContractProperty["access"],
): ContractProperty["access"] {
  if (existing === incoming) return existing;
  return "readwrite";
}

function mergeMethods(existing: ContractMethod, incoming: ContractMethod): ContractMethod {
  return {
    ...incoming,
    arguments: mergeMethodArguments(existing.arguments, incoming.arguments),
    returns: mergeObjectContractShapes(existing.returns, incoming.returns),
  };
}

function mergeMethodArguments(
  existing: ContractMethod["arguments"],
  incoming: ContractMethod["arguments"],
): ContractMethod["arguments"] {
  if (existing.kind === "open") return incoming;
  if (incoming.kind === "open") return existing;

  if (existing.shapes.length !== incoming.shapes.length) return incoming;

  const shapes: Shape[] = [];
  for (let index = 0; index < existing.shapes.length; index++) {
    shapes.push(mergeObjectContractShapes(existing.shapes[index]!, incoming.shapes[index]!));
  }

  return { kind: "exact", shapes };
}

function mergeEvents(existing: ContractEvent, incoming: ContractEvent): ContractEvent {
  return stableJson(existing) === stableJson(incoming)
    ? existing
    : incoming;
}

function mergeObjectContractShapes(existing: Shape, incoming: Shape): Shape {
  if (stableJson(existing) === stableJson(incoming)) return existing;
  if (existing.kind === "any") return incoming;
  if (incoming.kind === "any") return existing;
  if (isNullableOf(existing, incoming)) return existing;
  if (isNullableOf(incoming, existing)) return incoming;
  if (existing.kind === "array" && incoming.kind === "array") {
    return mergeArrayShapes(existing, incoming);
  }
  if (existing.kind === "object" && incoming.kind === "object") {
    return mergeObjectShapes(existing, incoming);
  }

  return incoming;
}

function isNullableOf(shape: Shape, inner: Shape): boolean {
  return shape.kind === "nullable" && stableJson(shape.inner) === stableJson(inner);
}

function mergeArrayShapes(
  existing: Extract<Shape, { kind: "array" }>,
  incoming: Extract<Shape, { kind: "array" }>,
): Shape {
  const item = mergeObjectContractShapes(existing.item, incoming.item);
  return { kind: "array", item };
}

function mergeObjectShapes(
  existing: Extract<Shape, { kind: "object" }>,
  incoming: Extract<Shape, { kind: "object" }>,
): Shape {
  const fields: Record<string, Shape> = { ...existing.fields };
  for (const [field, incomingShape] of Object.entries(incoming.fields)) {
    const existingShape = fields[field];
    if (existingShape === undefined) {
      fields[field] = incomingShape;
      continue;
    }

    fields[field] = mergeObjectContractShapes(existingShape, incomingShape);
  }

  const bothAreOpenObjects = existing.additional && incoming.additional;
  const noDeclaredFields = Object.keys(fields).length === 0;
  return {
    kind: "object",
    fields,
    additional: bothAreOpenObjects && noDeclaredFields,
  };
}

export function stableJson(value: unknown): string {
  if (Array.isArray(value)) return `[${value.map(stableJson).join(",")}]`;

  if (value !== null && typeof value === "object") {
    const entries = Object.entries(value as Record<string, unknown>)
      .sort(([left], [right]) => comparePropertyNames(left, right));
    return `{${entries.map(([key, item]) => `${JSON.stringify(key)}:${stableJson(item)}`).join(",")}}`;
  }

  return JSON.stringify(value);
}

function comparePropertyNames(left: string, right: string): number {
  if (left < right) return -1;
  if (left > right) return 1;
  return 0;
}
