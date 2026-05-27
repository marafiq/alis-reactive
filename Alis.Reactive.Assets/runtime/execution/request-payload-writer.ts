import type { RequestPayloadTarget, HttpMethod, PathSegment, RequestBodyFormat } from "../types";
import { toString } from "../core/shape-convert";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { plainObjectRecordFrom } from "../domain/object-record";
import { RuntimeShape } from "../domain/runtime-shape";

const log = scope("gather");

type GatheredArrayItem =
  | { readonly kind: "file"; readonly file: File }
  | { readonly kind: "value"; readonly value: unknown };

export interface GatherResult {
  urlParams: string[];
  routeParams: Record<string, string>;
  headers: Record<string, string>;
  body: Record<string, unknown> | FormData;
}

/** Writes gathered name/value pairs into query string, FormData, or JSON body. */
export interface RequestPayloadWriter {
  emitScalar(target: RequestPayloadTarget, value: unknown, shape: RuntimeShape): void;
  emitArray(target: RequestPayloadTarget, items: unknown[], itemShape: RuntimeShape): void;
}

export class GatheredRequestInput {
  private constructor(
    private readonly urlParams: string[],
    private readonly routeParams: Record<string, string>,
    private readonly headers: Record<string, string>,
    private readonly body: Record<string, unknown> | FormData,
    readonly writer: RequestPayloadWriter,
  ) {}

  static empty(): GatherResult {
    return { urlParams: [], routeParams: {}, headers: {}, body: {} };
  }

  static for(bodyFormat: RequestBodyFormat, method: HttpMethod): GatheredRequestInput {
    const urlParams: string[] = [];
    const routeParams: Record<string, string> = {};
    const headers: Record<string, string> = {};
    if (sendsInputInQueryString(method)) {
      return new GatheredRequestInput(urlParams, routeParams, headers, {}, createQueryStringWriter(urlParams));
    }

    switch (bodyFormat) {
      case "form-data": {
        const formData = new FormData();
        return new GatheredRequestInput(urlParams, routeParams, headers, formData, createFormDataWriter(formData));
      }
      case "json": {
        const body: Record<string, unknown> = {};
        return new GatheredRequestInput(urlParams, routeParams, headers, body, createJsonBodyWriter(body));
      }
      default:
        return assertNever(bodyFormat, "request body format");
    }
  }

  writeHeader(name: string, value: unknown, shape: RuntimeShape): void {
    this.headers[name] = requestScalarWireValue("header", name, value, shape);
  }

  writeRouteParameter(name: string, value: unknown, shape: RuntimeShape): void {
    const valueIsMissing = value === null || value === undefined;
    if (valueIsMissing) {
      throw new Error(`[alis] route param "${name}" evaluated to null; cannot build URL`);
    }

    this.routeParams[name] = requestScalarWireValue("route param", name, value, shape);
  }

  toResult(): GatherResult {
    return {
      urlParams: this.urlParams,
      routeParams: this.routeParams,
      headers: this.headers,
      body: this.body,
    };
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

export function writeGatheredValue(
  target: RequestPayloadTarget,
  raw: unknown,
  shape: RuntimeShape,
  writer: RequestPayloadWriter,
): void {
  const files = browserFiles(raw);
  if (files !== undefined) {
    writer.emitArray(target, files, shape);
    log.trace("file.emitted", { name: target.name, count: files.length });
    return;
  }

  if (Array.isArray(raw)) {
    writer.emitArray(target, raw, shape.item());
    return;
  }

  writer.emitScalar(target, raw, shape);
}

function createQueryStringWriter(urlParams: string[]): RequestPayloadWriter {
  return {
    emitScalar: (target, value, shape) => {
      const wire = shape.formatForWire(value);
      urlParams.push(`${encodeURIComponent(target.name)}=${encodeURIComponent(scalarWireValue(wire, target.name))}`);
    },
    emitArray: (target, items, itemShape) => {
      const gatheredItems = gatheredArrayItems(items);
      if (arrayContainsFile(gatheredItems)) throw new Error("[alis] File objects cannot be sent via GET");
      appendArrayItemsToQueryString(target.name, gatheredItems, itemShape, urlParams);
    },
  };
}

function createFormDataWriter(formData: FormData): RequestPayloadWriter {
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

function createJsonBodyWriter(body: Record<string, unknown>): RequestPayloadWriter {
  return {
    emitScalar: (target, value, shape) => {
      const wire = shape.formatForWire(value);
      assignJsonBodyValue(body, target, jsonBodyValue(wire));
    },
    emitArray: (target, items, itemShape) => {
      const gatheredItems = gatheredArrayItems(items);
      if (arrayContainsFile(gatheredItems)) throw new Error("[alis] File objects require form-data body format");
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

  const wrapper = plainObjectRecordFrom(item);
  if (wrapper !== undefined) {
    const rawFile = wrapper["rawFile"];
    if (rawFile instanceof File) return { kind: "file", file: rawFile };
  }

  return { kind: "value", value: item };
}

function arrayContainsFile(items: readonly GatheredArrayItem[]): boolean {
  return items.some(item => item.kind === "file");
}

function appendArrayItemsToQueryString(
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

function requestScalarWireValue(targetKind: "header" | "route param", name: string, value: unknown, shape: RuntimeShape): string {
  const wire = shape.formatForWire(value);
  const result = toString(wire);
  if (result.ok) return result.value;

  throw new Error(`[alis] ${targetKind} "${name}" cannot be serialized as a scalar: ${result.error}`);
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
    const nestedObject = plainObjectRecordFrom(value);
    if (nestedObject !== undefined) {
      parent = nestedObject;
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
