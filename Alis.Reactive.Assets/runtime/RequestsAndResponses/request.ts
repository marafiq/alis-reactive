import { Submission } from "../InputsAndIds/index";
import {
  assertNever, notBuilt, type Culture, type Expression, type HttpSend, type InputBinding, type MultipartPart, type PathPart, type PathStep, type ScalarWireForm, type SentBody,
  type SentEnum, type SentField, type SentHeader, type SentInput, type SentInputs, type Shape, type TimeZone, type WireForm,
} from "../Schema/index";
import { plugin } from "../Plugins/index";
import { Stop } from "../ValuesAndSources/index";
import { captureBusy, type CapturedBusy } from "./busy";
import { enumJson, fieldTexts, formDataPlace, held, invariantPlace, scalarText, type TextPlace } from "./wire";

export { Submission };

export interface PreparedRequest {
  readonly method: HttpSend["method"];
  readonly url: string;
  readonly template: string;
  readonly headers: Headers;
  readonly body: BodyInit | null;
  readonly request: object;
  readonly inputs: readonly SentInput[];
  readonly respondedJson: boolean;
  readonly retry: HttpSend["retry"];
  readonly releaseBusy: () => void;
}

export function prepare(send: HttpSend, zone: TimeZone, culture: Culture, busy: CapturedBusy = captureBusy(send.busy), findPlugin: typeof plugin = plugin): PreparedRequest {
  const contracts = new Map<HttpSend["contracts"][number], unknown>();
  for (const contract of send.contracts) contracts.set(contract, findPlugin(contract, ""));
  const request = {};
  const path = send.url.parts.map(part => pathText(part, zone, request)).join("");
  const template = routeTemplate(send.url.parts);
  const query = new URLSearchParams();
  appendFields(query, send.query.fields, zone, invariantPlace, request);
  appendInputs(query, send.query.inputs, zone, invariantPlace, request);
  const ownQuery = send.url.query === "" ? "" : "?" + send.url.query;
  const appended = query.toString();
  const url = path + ownQuery + (appended === "" ? "" : (ownQuery === "" ? "?" : "&") + appended);
  const headers = new Headers();
  for (const header of send.headers) appendHeader(headers, header, zone);
  const token = send.antiforgery;
  if (token.kind === "token" && token.place.kind === "header") headers.set(token.place.name, token.value);
  const field = token.kind === "token" && token.place.kind === "field" ? { name: token.place.name, value: token.value } : undefined;
  const body = bodyOf(send.body, headers, { zone, place: formDataPlace(culture), request, token: field });
  const inputs = [...sentInputs(send.query.inputs), ...sentInputs(send.body.kind === "none" ? noInputs : send.body.inputs)];
  const releaseBusy = busy.hold(contracts.get("busy"));
  return { method: send.method, url, template, headers, body, request, inputs, respondedJson: send.respondedJson, retry: send.retry, releaseBusy };
}

export function sentParts(send: HttpSend): HttpSend {
  return {
    ...send,
    query: { ...send.query, fields: send.query.fields.filter(field => field.when) },
    body: sentBody(send.body),
    headers: send.headers.filter(header => header.when),
  };
}

function sentBody(body: SentBody): SentBody {
  switch (body.kind) {
    case "none":
    case "text":
    case "file":
      return body;
    case "json":
      return { ...body, value: body.guarded.reduce<unknown>((value, member) => (member.when ? value : without(value, member.path)), body.value) };
    case "urlEncoded":
      return { ...body, fields: body.fields.filter(field => field.when) };
    case "multipart":
      return { ...body, parts: body.parts.filter(partSent) };
    default:
      return assertNever(body);
  }
}

function partSent(part: MultipartPart): boolean {
  switch (part.kind) {
    case "field":
      return part.field.when;
    case "file":
      return part.when;
    default:
      return assertNever(part);
  }
}

function without(holder: unknown, path: readonly string[]): unknown {
  const [step, ...past] = path;
  if (step === undefined || typeof holder !== "object" || holder === null || !Object.hasOwn(holder, step)) return holder;
  const copy = shallowCopy(holder);
  if (past.length === 0) Reflect.deleteProperty(copy, step);
  else Reflect.set(copy, step, without(Reflect.get(holder, step), past));
  return copy;
}

function shallowCopy(holder: object): object {
  return Array.isArray(holder) ? [...(holder as readonly unknown[])] : { ...holder };
}

function withEnum(holder: unknown, member: SentEnum): unknown {
  return replaced(holder, member.path, held => (held === undefined || held === null ? held : enumJson(held, member.form, member.name)));
}

function replaced(holder: unknown, path: readonly string[], replace: (held: unknown) => unknown): unknown {
  const [step, ...past] = path;
  if (step === undefined) return replace(holder);
  if (typeof holder !== "object" || holder === null || !Object.hasOwn(holder, step)) return holder;
  const copy = shallowCopy(holder);
  Reflect.set(copy, step, replaced(Reflect.get(holder, step), past, replace));
  return copy;
}

interface FormWriting {
  readonly zone: TimeZone;
  readonly place: TextPlace;
  readonly request: object;
  readonly token: { readonly name: string; readonly value: string } | undefined;
}

const noInputs: SentInputs = { container: { kind: "none" }, individual: [] };

function bodyOf(body: SentBody, headers: Headers, writing: FormWriting): BodyInit | null {
  switch (body.kind) {
    case "none":
      return null;
    case "json": {
      const value = withContainer(body.enums.reduce<unknown>((sent, member) => withEnum(sent, member), body.value), body.inputs);
      refuseFiles(value);
      headers.set("Content-Type", "application/json");
      return JSON.stringify(value);
    }
    case "urlEncoded": {
      const form = new URLSearchParams();
      appendFields(form, body.fields, writing.zone, writing.place, writing.request);
      appendInputs(form, body.inputs, writing.zone, writing.place, writing.request);
      if (writing.token !== undefined) form.set(writing.token.name, writing.token.value);
      return form;
    }
    case "multipart": {
      const form = new FormData();
      for (const part of body.parts) appendPart(form, part, writing);
      appendInputs(form, body.inputs, writing.zone, writing.place, writing.request);
      if (writing.token !== undefined) form.set(writing.token.name, writing.token.value);
      return form;
    }
    case "text": {
      const text = held(body.value);
      if (typeof text !== "string") throw new Stop("The request's text body must hold text.");
      return text;
    }
    case "file": {
      const file = held(body.value);
      if (!(file instanceof Blob)) throw new Stop("The request's file body must hold a file.");
      return file;
    }
    default:
      return assertNever(body);
  }
}

function appendFields(form: URLSearchParams | FormData, fields: readonly SentField[], zone: TimeZone, place: TextPlace, request: object): void {
  for (const field of fields) {
    const texts = fieldTexts(field, zone, place);
    for (const text of texts) form.append(field.name, text);
    if (texts.length > 0) write(request, field.path, snapshot(held(field.value)));
  }
}

function appendInputs(form: URLSearchParams | FormData, inputs: SentInputs, zone: TimeZone, place: TextPlace, request: object): void {
  const container = inputs.container;
  if (container.kind !== "captured") return;
  const sent = container.body;
  if (sent instanceof Submission) for (const [name, entry] of sent.entries) appendEntry(form, name, entry);
  const members = sent instanceof Submission ? sent.members : sent;
  for (const input of container.inputs) setInput(form, input, readAt(members, input.entry.binding.path), zone, place, request);
}

function appendEntry(form: URLSearchParams | FormData, name: string, entry: FormDataEntryValue): void {
  if (typeof entry === "string") {
    form.append(name, entry);
    return;
  }
  if (form instanceof URLSearchParams) throw new Stop("The request's form control '" + name + "' holds a file, which is sent only in multipart form data.");
  form.append(name, entry);
}

function setInput(form: URLSearchParams | FormData, input: SentInput, value: unknown, zone: TimeZone, place: TextPlace, request: object): void {
  const binding = input.entry.binding;
  const shape = convertedShape(binding);
  form.delete(input.name);
  if (shape.kind === "file" || (shape.kind === "array" && shape.item.kind === "file")) {
    appendFiles(form, input.name, value);
    return;
  }
  const field: SentField = { name: input.name, path: [], value: { kind: "held", value }, form: wireFormOf(shape, input.name), whenNothing: "empty", when: true };
  for (const text of fieldTexts(field, zone, place)) form.append(input.name, text);
  write(request, input.name.split("."), snapshot(value));
}

function appendFiles(form: URLSearchParams | FormData, name: string, value: unknown): void {
  if (value === undefined || value === null) return;
  const files = Array.isArray(value) ? value as readonly unknown[] : [value];
  for (const file of files) {
    if (!(file instanceof File)) throw new Stop("The request's file input '" + name + "' must hold a file in each item.");
    if (form instanceof URLSearchParams) throw new Stop("The request's file input '" + name + "' holds a file, which is sent only in multipart form data.");
    form.append(name, file);
  }
}

function withContainer(value: unknown, inputs: SentInputs): unknown {
  const container = inputs.container;
  if (container.kind !== "captured") return value;
  const sent: unknown = container.body;
  const members = sent instanceof Submission ? sent.members : sent;
  const merged = typeof value === "object" && value !== null ? structuredClone(value) : {};
  overwrite(merged, members);
  return merged;
}

function overwrite(holder: object, over: unknown): void {
  if (typeof over !== "object" || over === null) return;
  for (const [step, value] of Object.entries(over)) {
    const current: unknown = Reflect.get(holder, step);
    const nests = typeof value === "object" && value !== null && !Array.isArray(value) && typeof current === "object" && current !== null && !Array.isArray(current);
    if (nests) {
      overwrite(current, value);
      continue;
    }
    Object.defineProperty(holder, step, { value, enumerable: true, writable: true, configurable: true });
  }
}

function readAt(holder: unknown, path: readonly PathStep[]): unknown {
  let current = holder;
  for (const step of path) {
    if (typeof current !== "object" || current === null) return undefined;
    current = Reflect.get(current, step);
  }
  return current;
}

const unconverted = "The plan's input binding value is not coalesce(as(read, shape), blank), the one form C# writes it in.";

function convertedShape(binding: InputBinding): Shape {
  const value: Expression = binding.value;
  if (value.kind !== "op" || value.op !== "coalesce") throw new Error(unconverted);
  const [converted] = value.args;
  if (converted?.kind !== "as") throw new Error(unconverted);
  return converted.shape.kind === "nullable" ? converted.shape.inner : converted.shape;
}

function wireFormOf(shape: Shape, name: string): WireForm {
  if (shape.kind === "array") return { kind: "list", item: scalarFormOf(shape.item.kind === "nullable" ? shape.item.inner : shape.item, name), itemWhenNothing: "empty" };
  return scalarFormOf(shape, name);
}

function scalarFormOf(shape: Shape, name: string): ScalarWireForm {
  switch (shape.kind) {
    case "string":
      return { kind: "text" };
    case "number":
      return { kind: "number" };
    case "boolean":
      return { kind: "boolean" };
    case "date":
      return { kind: "date" };
    case "instant":
      return { kind: "instant" };
    case "file":
    case "any":
    case "array":
    case "nullable":
    case "object":
    case "enum":
      throw notBuilt("input '" + name + "' converts to the shape " + shape.kind + ", which no form data field writes as one text");
    default:
      return assertNever(shape);
  }
}

function appendPart(form: FormData, part: MultipartPart, writing: FormWriting): void {
  switch (part.kind) {
    case "field":
      appendFields(form, [part.field], writing.zone, writing.place, writing.request);
      return;
    case "file": {
      const files = held(part.value);
      if (files === undefined || files === null) return;
      for (const file of files as readonly unknown[]) {
        if (!(file instanceof File)) throw new Stop("The request's file part '" + part.name + "' must hold a file in each item.");
        form.append(part.name, file);
      }
      return;
    }
    default:
      assertNever(part);
  }
}

function refuseFiles(value: unknown): void {
  if (value instanceof Blob) throw new Stop("The request's JSON body holds a file, which is sent only in form data or as the whole body.");
  if (Array.isArray(value)) {
    for (const item of value as readonly unknown[]) refuseFiles(item);
    return;
  }
  if (typeof value === "object" && value !== null) for (const member of Object.values(value)) refuseFiles(member);
}

function sentInputs(capture: SentInputs): readonly SentInput[] {
  const inputs: SentInput[] = [];
  switch (capture.container.kind) {
    case "none":
      break;
    case "missing":
      throw new Stop("The request's container '" + capture.container.id + "' is missing.");
    case "captured":
      inputs.push(...capture.container.inputs);
      break;
    default:
      assertNever(capture.container);
  }
  for (const individual of capture.individual) {
    switch (individual.kind) {
      case "missing":
        throw new Stop("The request's input '" + individual.id + "' for '" + individual.name + "' is missing.");
      case "captured":
        inputs.push(individual.input);
        break;
      case "leftOut":
        break;
      default:
        assertNever(individual);
    }
  }
  return inputs;
}

function routeTemplate(parts: readonly PathPart[]): string {
  const written = parts.map(templateText).join("");
  const origin = /^https?:\/\/[^/]*/iu.exec(written);
  if (origin === null) return written;
  const path = written.slice(origin[0].length);
  return path === "" ? "/" : path;
}

function templateText(part: PathPart): string {
  switch (part.kind) {
    case "text":
      return part.text;
    case "member":
      return "{" + part.name + "}";
    default:
      return assertNever(part);
  }
}

function pathText(part: PathPart, zone: TimeZone, request: object): string {
  switch (part.kind) {
    case "text":
      return part.text;
    case "member": {
      const value = held(part.value);
      const text = value === undefined || value === null ? "" : scalarText(value, part.form, part.name, zone, invariantPlace);
      if (text === "" || text.includes("/")) throw new Stop("The request's path member '" + part.name + "' must hold nonempty text with no '/'.");
      write(request, [part.name], snapshot(value));
      try {
        return encodeURIComponent(text).replace(/[!'()*]/gu, character => "%" + character.charCodeAt(0).toString(16).toUpperCase());
      } catch {
        throw new Stop("The request's path member '" + part.name + "' is not valid Unicode text.");
      }
    }
    default:
      return assertNever(part);
  }
}

function appendHeader(headers: Headers, header: SentHeader, zone: TimeZone): void {
  const value = held(header.value);
  if (value === undefined || value === null) return;
  const text = scalarText(value, header.form, header.name, zone, invariantPlace);
  for (const character of text) {
    const code = character.charCodeAt(0);
    if (code !== 9 && (code < 32 || code > 126)) {
      throw new Stop("The request's header '" + header.name + "' must hold visible US-ASCII, spaces or tabs.");
    }
  }
  headers.append(header.name, text);
}

function snapshot(value: unknown): unknown {
  if (value === undefined || value === null) return null;
  if (Array.isArray(value)) return Array.from(value as readonly unknown[], snapshot);
  return structuredClone(value);
}

function write(holder: object, path: readonly PathStep[], value: unknown): void {
  const [step, ...past] = path;
  if (step === undefined) return;
  if (past.length === 0) {
    Object.defineProperty(holder, step, { value, enumerable: true, writable: true, configurable: true });
    return;
  }
  const child = Object.hasOwn(holder, step) ? Reflect.get(holder, step) as object : typeof past[0] === "number" ? [] : {};
  Object.defineProperty(holder, step, { value: child, enumerable: true, writable: true, configurable: true });
  write(child, past, value);
}
