import type { HttpMethod, Transport } from "../types";
import { toString } from "../core/shape-convert";
import { scope } from "../core/trace";
import { HttpRequestMethod } from "../domain/http-request-method";
import { PlainObjectRecord } from "../domain/object-record";
import { RuntimeShape } from "../domain/runtime-shape";

const log = scope("gather");

export interface GatherResult {
  urlParams: string[];
  body: Record<string, unknown> | FormData;
}

/** Transport strategies for emitting name/value pairs into GET, FormData, or JSON. */
export interface TransportStrategy {
  emitScalar(name: string, value: unknown, shape: RuntimeShape): void;
  emitArray(name: string, items: unknown[], itemShape: RuntimeShape): void;
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
    const requestMethod = HttpRequestMethod.from(method);
    if (requestMethod.sendsInputInQueryString()) {
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

export function emitGatheredValue(
  name: string,
  raw: unknown,
  shape: RuntimeShape,
  transport: TransportStrategy,
): void {
  GatheredValue.from(name, raw, shape).emitInto(transport);
}

function createGetTransport(urlParams: string[]): TransportStrategy {
  return {
    emitScalar: (name, value, shape) => {
      const wire = shape.formatForWire(value);
      urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(GatherScalarWireValue.from(wire, name))}`);
    },
    emitArray: (name, items, itemShape) => {
      const gatheredItems = GatheredArrayItems.from(items);
      if (gatheredItems.containsFile) throw new Error("[alis] File objects cannot be sent via GET");
      gatheredItems.emitToQueryString(name, itemShape, urlParams);
    },
  };
}

function createFormDataTransport(formData: FormData): TransportStrategy {
  return {
    emitScalar: (name, value, shape) => {
      const wire = shape.formatForWire(value);
      formData.append(name, GatherScalarWireValue.from(wire, name));
    },
    emitArray: (name, items, itemShape) => {
      GatheredArrayItems.from(items).appendToFormData(name, itemShape, formData);
    },
  };
}

function createJsonTransport(body: Record<string, unknown>): TransportStrategy {
  return {
    emitScalar: (name, value, shape) => {
      const wire = shape.formatForWire(value);
      JsonBodyPath.from(name).assign(body, JsonBodyValue.fromWire(wire));
    },
    emitArray: (name, items, itemShape) => {
      const gatheredItems = GatheredArrayItems.from(items);
      if (gatheredItems.containsFile) throw new Error("[alis] File objects require transport: form-data");
      const wireItems = gatheredItems.toJsonValue(itemShape);
      JsonBodyPath.from(name).assign(body, wireItems);
    },
  };
}

class GatheredValue {
  private constructor(
    private readonly name: string,
    private readonly raw: unknown,
    private readonly shape: RuntimeShape,
  ) {}

  static from(name: string, raw: unknown, shape: RuntimeShape): GatheredValue {
    return new GatheredValue(name, raw, shape);
  }

  emitInto(transport: TransportStrategy): void {
    const fileList = BrowserFileList.tryFrom(this.raw);
    const rawValueIsBrowserFileList = fileList !== undefined;
    if (rawValueIsBrowserFileList) {
      transport.emitArray(this.name, fileList.files(), this.shape);
      log.trace("file.emitted", { name: this.name, count: fileList.count });
      return;
    }

    const arrayValue = GatheredArrayValue.tryFrom(this.raw, this.shape);
    const rawValueIsArray = arrayValue !== undefined;
    if (rawValueIsArray) {
      transport.emitArray(this.name, arrayValue.items, arrayValue.itemShape);
      return;
    }

    transport.emitScalar(this.name, this.raw, this.shape);
  }
}

class BrowserFileList {
  private constructor(
    private readonly value: FileList,
    readonly count: number,
  ) {}

  static tryFrom(raw: unknown): BrowserFileList | undefined {
    const browserExposesFileList = typeof FileList !== "undefined";
    if (!browserExposesFileList) return undefined;

    const rawIsFileList = raw instanceof FileList;
    if (!rawIsFileList) return undefined;

    return new BrowserFileList(raw, raw.length);
  }

  files(): File[] {
    return Array.from(this.value);
  }
}

class GatheredArrayValue {
  private constructor(
    readonly items: unknown[],
    readonly itemShape: RuntimeShape,
  ) {}

  static tryFrom(raw: unknown, shape: RuntimeShape): GatheredArrayValue | undefined {
    const rawIsArray = Array.isArray(raw);
    if (!rawIsArray) return undefined;

    return new GatheredArrayValue(raw, shape.item());
  }
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
    return JsonArrayBodyValue.fromItems(
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
    urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(GatherScalarWireValue.from(wire, name))}`);
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
    formData.append(name, GatherScalarWireValue.from(wire, name));
  }
}

class GatherScalarWireValue {
  static from(value: unknown, name: string): string {
    const result = toString(value);
    if (result.ok) return result.value;

    throw new Error(`[alis] gather value "${name}" cannot be serialized as a scalar: ${result.error}`);
  }
}

class JsonBodyValue {
  static fromWire(wireValue: unknown): unknown {
    const emptyTextRepresentsClearedField = wireValue === "";
    if (emptyTextRepresentsClearedField) return null;

    return wireValue;
  }
}

class JsonArrayBodyValue {
  static fromItems(items: unknown[], itemShape: RuntimeShape): unknown[] {
    if (!itemShape.isDeclared) return items;

    return items.map(item => itemShape.formatForWire(item));
  }
}

class JsonBodyPath {
  private constructor(
    private readonly key: string,
    private readonly parts: [string, ...string[]],
  ) {}

  static from(key: string): JsonBodyPath {
    const parts = key.split(".");
    const keyContainsEmptySegment = parts.some(part => part.length === 0);
    if (keyContainsEmptySegment) {
      throw new Error(`[alis] gather key "${key}" contains an empty path segment`);
    }

    return new JsonBodyPath(key, parts as [string, ...string[]]);
  }

  assign(body: Record<string, unknown>, value: unknown): void {
    const pathTargetsRootField = this.parts.length === 1;
    if (pathTargetsRootField) {
      JsonBodyLeaf.from(body, this.rootPart(), this.key).assign(value);
      return;
    }

    const parent = this.parentObject(body);
    JsonBodyLeaf.from(parent, this.lastPart(), this.key).assign(value);
  }

  overlaps(other: JsonBodyPath): boolean {
    return this.isPrefixOf(other) || other.isPrefixOf(this);
  }

  private isPrefixOf(other: JsonBodyPath): boolean {
    if (this.parts.length > other.parts.length) return false;

    return this.parts.every((part, index) => part === other.parts[index]);
  }

  private parentObject(body: Record<string, unknown>): Record<string, unknown> {
    let current = body;
    const parentPath = this.parts.slice(0, -1);
    const walkedPath: string[] = [];

    for (const segment of parentPath) {
      walkedPath.push(segment);
      current = JsonBodySlot.from(current, segment, this.key, walkedPath.join(".")).ensureObject();
    }

    return current;
  }

  private rootPart(): string {
    return this.parts[0];
  }

  private lastPart(): string {
    const part = this.parts[this.parts.length - 1];
    if (part === undefined) {
      throw new Error("[alis] gather path is empty");
    }

    return part;
  }
}

class JsonBodySlot {
  private constructor(
    private readonly parent: Record<string, unknown>,
    private readonly segment: string,
    private readonly ownerKey: string,
    private readonly segmentPath: string,
  ) {}

  static from(
    parent: Record<string, unknown>,
    segment: string,
    ownerKey: string,
    segmentPath: string,
  ): JsonBodySlot {
    return new JsonBodySlot(parent, segment, ownerKey, segmentPath);
  }

  ensureObject(): Record<string, unknown> {
    const value = this.parent[this.segment];
    const segmentHasNoValue = !(this.segment in this.parent);
    if (segmentHasNoValue) {
      this.parent[this.segment] = {};
      return this.parent[this.segment] as Record<string, unknown>;
    }

    const nestedObject = PlainObjectRecord.tryFrom(value);
    if (nestedObject !== undefined) return nestedObject.raw;

    throw new Error(
      `[alis] gather key "${this.ownerKey}" conflicts at "${this.segmentPath}": ` +
      "an existing scalar value cannot hold nested fields. " +
      `Use either "${this.segmentPath}" or "${this.segmentPath}.*" fields, not both.`
    );
  }
}

class JsonBodyLeaf {
  private constructor(
    private readonly parent: Record<string, unknown>,
    private readonly segment: string,
    private readonly ownerKey: string,
  ) {}

  static from(parent: Record<string, unknown>, segment: string, ownerKey: string): JsonBodyLeaf {
    return new JsonBodyLeaf(parent, segment, ownerKey);
  }

  assign(value: unknown): void {
    const existingValue = this.parent[this.segment];
    const segmentHasValue = this.segment in this.parent;
    const existingValueIsNestedObject = PlainObjectRecord.tryFrom(existingValue) !== undefined;
    const incomingValueIsNestedObject = PlainObjectRecord.tryFrom(value) !== undefined;
    const assignmentWouldReplaceNestedFields =
      segmentHasValue
      && existingValueIsNestedObject
      && !incomingValueIsNestedObject;
    if (assignmentWouldReplaceNestedFields) {
      throw new Error(
        `[alis] gather key "${this.ownerKey}" conflicts at "${this.segment}": ` +
        "nested fields were already assigned under this key. " +
        `Use either "${this.segment}" or "${this.segment}.*" fields, not both.`
      );
    }

    this.parent[this.segment] = value;
  }
}
