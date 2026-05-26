import type { RequestPayloadTarget, HttpMethod, PathSegment, Transport } from "../types";
import { toString } from "../core/shape-convert";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { PlainObjectRecord } from "../domain/object-record";
import { RuntimeShape } from "../domain/runtime-shape";

const log = scope("gather");

type GatheredArrayItem =
  | { readonly kind: "file"; readonly file: File }
  | { readonly kind: "value"; readonly value: unknown };

export interface GatherResult {
  urlParams: string[];
  body: Record<string, unknown> | FormData;
}

/** Transport strategies for emitting name/value pairs into GET, FormData, or JSON. */
export interface TransportStrategy {
  emitScalar(target: RequestPayloadTarget, value: unknown, shape: RuntimeShape): void;
  emitArray(target: RequestPayloadTarget, items: unknown[], itemShape: RuntimeShape): void;
}

export class GatherOutput {
  private constructor(
    private readonly urlParams: string[],
    private readonly body: Record<string, unknown> | FormData,
    readonly transport: TransportStrategy,
  ) {}

  static empty(): GatherResult {
    return { urlParams: [], body: {} };
  }

  static for(requestTransport: Transport, method: HttpMethod): GatherOutput {
    const urlParams: string[] = [];
    if (sendsInputInQueryString(method)) {
      return new GatherOutput(urlParams, {}, createGetTransport(urlParams));
    }

    if (requestTransport === "form-data") {
      const formData = new FormData();
      return new GatherOutput(urlParams, formData, createFormDataTransport(formData));
    }

    const body: Record<string, unknown> = {};
    return new GatherOutput(urlParams, body, createJsonTransport(body));
  }

  toResult(): GatherResult {
    return { urlParams: this.urlParams, body: this.body };
  }
}

function sendsInputInQueryString(method: HttpMethod): boolean {
  switch (method) {
    case "GET":
      return true;
    case "POST":
    case "PUT":
    case "DELETE":
    case "PATCH":
      return false;
    default:
      return assertNever(method, "HTTP method");
  }
}

export function emitGatheredValue(
  target: RequestPayloadTarget,
  raw: unknown,
  shape: RuntimeShape,
  transport: TransportStrategy,
): void {
  const files = browserFiles(raw);
  if (files !== undefined) {
    transport.emitArray(target, files, shape);
    log.trace("file.emitted", { name: target.name, count: files.length });
    return;
  }

  if (Array.isArray(raw)) {
    transport.emitArray(target, raw, shape.item());
    return;
  }

  transport.emitScalar(target, raw, shape);
}

function createGetTransport(urlParams: string[]): TransportStrategy {
  return {
    emitScalar: (target, value, shape) => {
      const wire = shape.formatForWire(value);
      urlParams.push(`${encodeURIComponent(target.name)}=${encodeURIComponent(scalarWireValue(wire, target.name))}`);
    },
    emitArray: (target, items, itemShape) => {
      const gatheredItems = gatheredArrayItems(items);
      if (arrayContainsFile(gatheredItems)) throw new Error("[alis] File objects cannot be sent via GET");
      emitArrayItemsToQueryString(target.name, gatheredItems, itemShape, urlParams);
    },
  };
}

function createFormDataTransport(formData: FormData): TransportStrategy {
  return {
    emitScalar: (target, value, shape) => {
      const wire = shape.formatForWire(value);
      formData.append(target.name, scalarWireValue(wire, target.name));
    },
    emitArray: (target, items, itemShape) => {
      appendArrayItemsToFormData(target.name, gatheredArrayItems(items), itemShape, formData);
    },
  };
}

function createJsonTransport(body: Record<string, unknown>): TransportStrategy {
  return {
    emitScalar: (target, value, shape) => {
      const wire = shape.formatForWire(value);
      assignJsonBodyValue(body, target, jsonBodyValue(wire));
    },
    emitArray: (target, items, itemShape) => {
      const gatheredItems = gatheredArrayItems(items);
      if (arrayContainsFile(gatheredItems)) throw new Error("[alis] File objects require transport: form-data");
      const wireItems = jsonArrayBodyValue(gatheredItems.map(rawArrayItemValue), itemShape);
      assignJsonBodyValue(body, target, wireItems);
    },
  };
}

function browserFiles(raw: unknown): File[] | undefined {
  const browserExposesFileList = typeof FileList !== "undefined";
  if (!browserExposesFileList) return undefined;

  const rawIsFileList = raw instanceof FileList;
  if (!rawIsFileList) return undefined;

  return Array.from(raw);
}

function gatheredArrayItems(items: unknown[]): GatheredArrayItem[] {
  return items.map(gatheredArrayItem);
}

function gatheredArrayItem(item: unknown): GatheredArrayItem {
  if (item instanceof File) return { kind: "file", file: item };

  const wrapper = PlainObjectRecord.tryFrom(item);
  if (wrapper !== undefined) {
    const rawFile = wrapper.get("rawFile");
    if (rawFile instanceof File) return { kind: "file", file: rawFile };
  }

  return { kind: "value", value: item };
}

function arrayContainsFile(items: readonly GatheredArrayItem[]): boolean {
  return items.some(item => item.kind === "file");
}

function emitArrayItemsToQueryString(
  name: string,
  items: readonly GatheredArrayItem[],
  itemShape: RuntimeShape,
  urlParams: string[],
): void {
  for (const item of items) {
    const wire = itemShape.formatForWire(rawArrayItemValue(item));
    urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(scalarWireValue(wire, name))}`);
  }
}

function appendArrayItemsToFormData(
  name: string,
  items: readonly GatheredArrayItem[],
  itemShape: RuntimeShape,
  formData: FormData,
): void {
  for (const item of items) {
    appendArrayItemToFormData(name, item, itemShape, formData);
  }
}

function appendArrayItemToFormData(
  name: string,
  item: GatheredArrayItem,
  itemShape: RuntimeShape,
  formData: FormData,
): void {
  switch (item.kind) {
    case "file":
      formData.append(name, item.file, item.file.name);
      return;
    case "value": {
      const wire = itemShape.formatForWire(item.value);
      formData.append(name, scalarWireValue(wire, name));
      return;
    }
    default:
      return assertNever(item, "gathered array item");
  }
}

function rawArrayItemValue(item: GatheredArrayItem): unknown {
  switch (item.kind) {
    case "file":
      return item.file;
    case "value":
      return item.value;
    default:
      return assertNever(item, "gathered array item");
  }
}

function scalarWireValue(value: unknown, name: string): string {
  const result = toString(value);
  if (result.ok) return result.value;

  throw new Error(`[alis] gather value "${name}" cannot be serialized as a scalar: ${result.error}`);
}

function jsonBodyValue(wireValue: unknown): unknown {
  const emptyTextRepresentsClearedField = wireValue === "";
  if (emptyTextRepresentsClearedField) return null;

  return wireValue;
}

function jsonArrayBodyValue(items: unknown[], itemShape: RuntimeShape): unknown[] {
  if (!itemShape.isDeclared) return items;

  return items.map(item => itemShape.formatForWire(item));
}

function assignJsonBodyValue(
  body: Record<string, unknown>,
  target: RequestPayloadTarget,
  value: unknown,
): void {
  const segments = target.path.map(bodySegment);
  const first = segments[0];
  if (first === undefined) {
    throw new Error(`[alis] gather key "${target.name}" contains no path segments`);
  }

  let parent = body;
  for (const segment of segments.slice(0, -1)) {
    const value = parent[segment];
    const nestedObject = PlainObjectRecord.tryFrom(value);
    if (nestedObject !== undefined) {
      parent = nestedObject.raw;
      continue;
    }

    parent[segment] = {};
    parent = parent[segment] as Record<string, unknown>;
  }

  parent[segments[segments.length - 1]!] = value;
}

function bodySegment(segment: PathSegment): string | number {
  switch (segment.kind) {
    case "property":
      return segment.name;
    case "index":
      return segment.index;
    default:
      return assertNever(segment, "path segment");
  }
}
