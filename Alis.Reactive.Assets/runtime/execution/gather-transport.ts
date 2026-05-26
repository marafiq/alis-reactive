import type { RequestPayloadTarget, HttpMethod, PathSegment, Transport } from "../types";
import { toString } from "../core/shape-convert";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { PlainObjectRecord } from "../domain/object-record";
import { RuntimeShape } from "../domain/runtime-shape";

const log = scope("gather");

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
      const gatheredItems = GatheredArrayItems.from(items);
      if (gatheredItems.containsFile) throw new Error("[alis] File objects cannot be sent via GET");
      gatheredItems.emitToQueryString(target.name, itemShape, urlParams);
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
      GatheredArrayItems.from(items).appendToFormData(target.name, itemShape, formData);
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
      const gatheredItems = GatheredArrayItems.from(items);
      if (gatheredItems.containsFile) throw new Error("[alis] File objects require transport: form-data");
      const wireItems = gatheredItems.toJsonValue(itemShape);
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

class GatheredArrayItems {
  private constructor(private readonly items: GatheredArrayItem[]) {}

  static from(items: unknown[]): GatheredArrayItems {
    return new GatheredArrayItems(items.map(item => GatheredArrayItem.from(item)));
  }

  get containsFile(): boolean {
    return this.items.some(item => item.containsFile);
  }

  emitToQueryString(name: string, itemShape: RuntimeShape, urlParams: string[]): void {
    for (const item of this.items) {
      item.emitToQueryString(name, itemShape, urlParams);
    }
  }

  appendToFormData(name: string, itemShape: RuntimeShape, formData: FormData): void {
    for (const item of this.items) {
      item.appendToFormData(name, itemShape, formData);
    }
  }

  toJsonValue(itemShape: RuntimeShape): unknown[] {
    return jsonArrayBodyValue(
      this.items.map(item => item.rawValue),
      itemShape
    );
  }
}

abstract class GatheredArrayItem {
  static from(item: unknown): GatheredArrayItem {
    const itemIsBrowserFile = item instanceof File;
    if (itemIsBrowserFile) return new UploadedFileArrayItem(item);

    const wrapper = PlainObjectRecord.tryFrom(item);
    const itemCanWrapBrowserFile = wrapper !== undefined;
    if (itemCanWrapBrowserFile) {
      const rawFile = wrapper.get("rawFile");
      const itemWrapsBrowserFile = rawFile instanceof File;
      if (itemWrapsBrowserFile) return new UploadedFileArrayItem(rawFile);
    }

    return new SerializableArrayItem(item);
  }

  abstract get containsFile(): boolean;
  abstract get rawValue(): unknown;
  abstract appendToFormData(name: string, itemShape: RuntimeShape, formData: FormData): void;

  emitToQueryString(name: string, itemShape: RuntimeShape, urlParams: string[]): void {
    const wire = itemShape.formatForWire(this.rawValue);
    urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(scalarWireValue(wire, name))}`);
  }
}

class UploadedFileArrayItem extends GatheredArrayItem {
  constructor(private readonly file: File) {
    super();
  }

  get containsFile(): boolean {
    return true;
  }

  get rawValue(): unknown {
    return this.file;
  }

  appendToFormData(name: string, _itemShape: RuntimeShape, formData: FormData): void {
    formData.append(name, this.file, this.file.name);
  }
}

class SerializableArrayItem extends GatheredArrayItem {
  constructor(private readonly value: unknown) {
    super();
  }

  get containsFile(): boolean {
    return false;
  }

  get rawValue(): unknown {
    return this.value;
  }

  appendToFormData(name: string, itemShape: RuntimeShape, formData: FormData): void {
    const wire = itemShape.formatForWire(this.value);
    formData.append(name, scalarWireValue(wire, name));
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
