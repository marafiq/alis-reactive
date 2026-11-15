import {
  assertNever, notBuilt, type Culture, type HttpSend, type MultipartPart, type PathPart, type PathStep, type SentBody, type SentField, type SentHeader, type SentInput,
  type SentInputs, type TimeZone,
} from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";
import { fieldTexts, formDataPlace, held, invariantPlace, scalarText, type TextPlace } from "./wire";

export interface PreparedRequest {
  readonly method: HttpSend["method"];
  readonly url: string;
  readonly headers: Headers;
  readonly body: BodyInit | null;
  readonly request: object;
  readonly inputs: readonly SentInput[];
  readonly respondedJson: boolean;
}

export function prepare(send: HttpSend, zone: TimeZone, culture: Culture): PreparedRequest {
  noRetry(send.retry);
  noBusy(send.busy);
  for (const contract of send.contracts) {
    switch (contract) {
      case "busy":
        throw notBuilt("the busy contract: U18's busy indicator");
      case "notice":
        throw notBuilt("the notice contract: U18's notices");
      default:
        assertNever(contract);
    }
  }
  const request = {};
  const path = send.url.parts.map(part => pathText(part, zone, request)).join("");
  const query = new URLSearchParams();
  appendFields(query, send.query.fields, zone, invariantPlace, request);
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
  return { method: send.method, url, headers, body, request, inputs, respondedJson: send.respondedJson };
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
    case "json":
      refuseFiles(body.value);
      headers.set("Content-Type", "application/json");
      return JSON.stringify(body.value);
    case "urlEncoded": {
      const form = new URLSearchParams();
      appendFields(form, body.fields, writing.zone, writing.place, writing.request);
      if (writing.token !== undefined) form.set(writing.token.name, writing.token.value);
      return form;
    }
    case "multipart": {
      const form = new FormData();
      for (const part of body.parts) appendPart(form, part, writing);
      if (writing.token !== undefined) form.set(writing.token.name, writing.token.value);
      return form;
    }
    case "text": {
      const text = held(body.value);
      if (text === undefined || text === null) throw notBuilt("a text body that reads nothing: no line states what such a request sends");
      if (typeof text !== "string") throw new Stop("The request's text body must hold text.");
      return text;
    }
    case "file": {
      const file = held(body.value);
      if (file === undefined || file === null) throw notBuilt("a file body that reads nothing: no line states what such a request sends");
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

function noRetry(retry: HttpSend["retry"]): void {
  switch (retry.kind) {
    case "none":
      return;
    case "standard":
      throw notBuilt("the standard retry policy: U18's retry");
    default:
      assertNever(retry);
  }
}

function noBusy(busy: HttpSend["busy"]): void {
  switch (busy.kind) {
    case "none":
      return;
    case "page":
      throw notBuilt("page busy: U18's busy indicator");
    case "targets":
      throw notBuilt("target busy: U18's busy indicator");
    default:
      assertNever(busy);
  }
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
      default:
        assertNever(individual);
    }
  }
  return inputs;
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
