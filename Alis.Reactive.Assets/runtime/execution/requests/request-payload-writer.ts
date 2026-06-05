// Request input writing is where gathered body-field assignments cross into query string,
// JSON body, or FormData. File inputs may arrive as FileList or Syncfusion
// items carrying the browser File in rawFile.

import type { RequestPayloadTarget, HttpMethod, PathSegment, RequestBodyFormat } from "../../types/index";
import { toString } from "../../shared/shape-convert";
import { scope } from "../../diagnostics/trace";
import { assertNever } from "../../shared/assert-never";
import { plainObjectRecordFrom } from "../../browser-objects/object-record";
import { RuntimeShape } from "../../browser-objects/runtime-shape";

const log = scope("gather");

type RequestInputArrayItem =
  | { readonly kind: "file"; readonly file: File }
  | { readonly kind: "value"; readonly value: unknown };

export interface ResolvedRequestInput {
  urlParams: string[];
  routeParams: Record<string, string>;
  headers: Record<string, string>;
  body: Record<string, unknown> | FormData;
}

interface RequestInputWriter {
  emitScalar(target: RequestPayloadTarget, value: unknown, shape: RuntimeShape): void;
  emitArray(target: RequestPayloadTarget, items: unknown[], itemShape: RuntimeShape): void;
}

export function requestPayloadWriterFor(
  requestInput: ResolvedRequestInput,
  bodyFormat: RequestBodyFormat,
  method: HttpMethod,
): RequestInputWriter {
  if (sendsInputInQueryString(method)) {
    return createQueryStringWriter(requestInput.urlParams);
  }

  switch (bodyFormat) {
    case "form-data":
      requestInput.body = new FormData();
      return createFormDataWriter(requestInput.body);
    case "json":
      requestInput.body = {};
      return createJsonBodyWriter(requestInput.body);
    default:
      return assertNever(bodyFormat, "request body format");
  }
}

function sendsInputInQueryString(method: HttpMethod): boolean {
  switch (method) {
    case "GET":
      return true;
    case "POST":
    case "PUT":
    case "DELETE":
      return false;
    default:
      return assertNever(method, "HTTP method");
  }
}

export function writeRequestPayloadValue(
  target: RequestPayloadTarget,
  gatheredValue: unknown,
  shape: RuntimeShape,
  writer: RequestInputWriter,
): void {
  const browserFileListItems = filesFromBrowserFileList(gatheredValue);
  if (browserFileListItems !== undefined) {
    writer.emitArray(target, browserFileListItems, shape);
    log.trace("file.emitted", { name: target.name, count: browserFileListItems.length });
    return;
  }

  if (Array.isArray(gatheredValue)) {
    writer.emitArray(target, gatheredValue, shape.item());
    return;
  }

  writer.emitScalar(target, gatheredValue, shape);
}

function createQueryStringWriter(urlParams: string[]): RequestInputWriter {
  return {
    emitScalar: (target, value, shape) => {
      const wire = shape.formatForWire(value);
      urlParams.push(`${encodeURIComponent(target.name)}=${encodeURIComponent(scalarWireValue(wire, target.name))}`);
    },
    emitArray: (target, items, itemShape) => {
      const inputItems = requestInputArrayItems(items);
      if (arrayContainsFile(inputItems)) throw new Error("[alis] File objects cannot be sent via GET");
      appendArrayItemsToQueryString(target.name, inputItems, itemShape, urlParams);
    },
  };
}

function createFormDataWriter(formData: FormData): RequestInputWriter {
  return {
    emitScalar: (target, value, shape) => {
      const wire = shape.formatForWire(value);
      formData.append(target.name, scalarWireValue(wire, target.name));
    },
    emitArray: (target, items, itemShape) => {
      appendArrayItemsToFormData(target.name, requestInputArrayItems(items), itemShape, formData);
    },
  };
}

function createJsonBodyWriter(body: Record<string, unknown>): RequestInputWriter {
  return {
    emitScalar: (target, value, shape) => {
      const wire = shape.formatForWire(value);
      assignJsonBodyValue(body, target, jsonBodyValue(wire));
    },
    emitArray: (target, items, itemShape) => {
      const inputItems = requestInputArrayItems(items);
      if (arrayContainsFile(inputItems)) throw new Error("[alis] File objects require form-data body format");
      const wireItems = jsonArrayBodyValue(inputItems.map(requestInputArrayItemValue), itemShape);
      assignJsonBodyValue(body, target, wireItems);
    },
  };
}

function filesFromBrowserFileList(value: unknown): File[] | undefined {
  const hasBrowserFileListConstructor = typeof FileList !== "undefined";
  if (!hasBrowserFileListConstructor) return undefined;

  const valueIsFileList = value instanceof FileList;
  if (!valueIsFileList) return undefined;

  return Array.from(value);
}

function requestInputArrayItems(items: unknown[]): RequestInputArrayItem[] {
  return items.map(requestInputArrayItem);
}

function requestInputArrayItem(item: unknown): RequestInputArrayItem {
  if (item instanceof File) return { kind: "file", file: item };

  const syncfusionFileItem = plainObjectRecordFrom(item);
  if (syncfusionFileItem !== undefined) {
    const rawFile = syncfusionFileItem["rawFile"];
    if (rawFile instanceof File) return { kind: "file", file: rawFile };
  }

  return { kind: "value", value: item };
}

function arrayContainsFile(items: readonly RequestInputArrayItem[]): boolean {
  return items.some(item => item.kind === "file");
}

function appendArrayItemsToQueryString(
  name: string,
  items: readonly RequestInputArrayItem[],
  itemShape: RuntimeShape,
  urlParams: string[],
): void {
  for (const item of items) {
    const wire = itemShape.formatForWire(requestInputArrayItemValue(item));
    urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(scalarWireValue(wire, name))}`);
  }
}

function appendArrayItemsToFormData(
  name: string,
  items: readonly RequestInputArrayItem[],
  itemShape: RuntimeShape,
  formData: FormData,
): void {
  for (const item of items) {
    appendArrayItemToFormData(name, item, itemShape, formData);
  }
}

function appendArrayItemToFormData(
  name: string,
  item: RequestInputArrayItem,
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
      return assertNever(item, "request input array item");
  }
}

function requestInputArrayItemValue(item: RequestInputArrayItem): unknown {
  switch (item.kind) {
    case "file":
      return item.file;
    case "value":
      return item.value;
    default:
      return assertNever(item, "request input array item");
  }
}

function scalarWireValue(value: unknown, name: string): string {
  const stringConversion = toString(value);
  if (stringConversion.ok) return stringConversion.value;

  throw new Error(`[alis] gather value "${name}" cannot be serialized as a scalar: ${stringConversion.error}`);
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
  let parent = body;
  for (const segment of segments.slice(0, -1)) {
    const existingChild = parent[segment];
    const nestedObject = plainObjectRecordFrom(existingChild);
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
