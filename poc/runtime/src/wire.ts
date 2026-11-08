import type { ListForm, ScalarKind, WireForm } from "./plan";
import { assertNever, Stop } from "./read";
import { dateOf, dateText, type DateText } from "./shapes";
import { invariantText } from "./sums";

export function wireTexts(member: string, held: unknown, form: WireForm): readonly string[] | undefined {
  if (held === null || held === undefined) return undefined;
  const place = "The request member '" + member + "'";
  if (form.kind !== "list") return [scalarText(place, held, form.kind)];
  if (!Array.isArray(held)) throw notInItsForm(place, "a list", held);
  return held.map((item: unknown, index: number) => itemText(member, item, index, form));
}

function itemText(member: string, item: unknown, index: number, form: ListForm): string {
  const place = "Item " + String(index + 1) + " of the request member '" + member + "'";
  if (item !== null && item !== undefined) return scalarText(place, item, form.item);
  switch (form.itemWhenNothing) {
    case "empty":
      return "";
    case "stop":
      throw new Stop(place + " reads nothing, and no text the endpoint reads binds it as nothing, so the request is not sent.");
    default:
      return assertNever(form.itemWhenNothing);
  }
}

function scalarText(place: string, held: NonNullable<unknown>, kind: ScalarKind): string {
  switch (kind) {
    case "text":
      if (typeof held === "string") return held;
      throw notInItsForm(place, "text", held);
    case "number":
      if (typeof held === "number" && Number.isFinite(held)) return invariantText(held);
      throw notInItsForm(place, "a number", held);
    case "boolean":
      if (typeof held === "boolean") return String(held);
      throw notInItsForm(place, "true or false", held);
    case "written":
      if (typeof held === "string") return held;
      if (typeof held === "number" && Number.isFinite(held)) return invariantText(held);
      throw notInItsForm(place, "text or a number", held);
    case "date":
      return dateWire(clockOf(place, held));
    case "instant":
      return instantWire(momentOf(place, held));
    default:
      return assertNever(kind);
  }
}

function clockOf(place: string, held: NonNullable<unknown>): DateText {
  if (isMoment(held)) return localClock(held);
  const text = typeof held === "string" ? dateText(held) : undefined;
  if (text === undefined) throw notInItsForm(place, "a date", held);
  return text;
}

function momentOf(place: string, held: NonNullable<unknown>): Date {
  if (isMoment(held)) return held;
  const text = typeof held === "string" ? dateText(held) : undefined;
  if (text === undefined) throw notInItsForm(place, "an instant", held);
  return dateOf(text);
}

function isMoment(held: unknown): held is Date {
  return held instanceof Date && !Number.isNaN(held.getTime());
}

function localClock(moment: Date): DateText {
  return {
    year: moment.getFullYear(),
    month: moment.getMonth() + 1,
    day: moment.getDate(),
    hour: moment.getHours(),
    minute: moment.getMinutes(),
    second: moment.getSeconds(),
    millisecond: moment.getMilliseconds(),
    offset: { kind: "none" },
  };
}

function dateWire(clock: DateText): string {
  const midnight = clock.hour === 0 && clock.minute === 0 && clock.second === 0 && clock.millisecond === 0;
  return midnight ? dayOf(clock) : dayOf(clock) + "T" + timeOf(clock);
}

function instantWire(moment: Date): string {
  const clock = localClock(moment);
  return dayOf(clock) + "T" + timeOf(clock) + offsetOf(-moment.getTimezoneOffset());
}

function dayOf(clock: DateText): string {
  return String(clock.year).padStart(4, "0") + "-" + two(clock.month) + "-" + two(clock.day);
}

function timeOf(clock: DateText): string {
  const fraction = clock.millisecond === 0 ? "" : "." + String(clock.millisecond).padStart(3, "0");
  return two(clock.hour) + ":" + two(clock.minute) + ":" + two(clock.second) + fraction;
}

function offsetOf(east: number): string {
  if (east === 0) return "Z";
  const magnitude = Math.abs(east);
  return (east > 0 ? "+" : "-") + two(Math.floor(magnitude / 60)) + ":" + two(magnitude % 60);
}

function two(value: number): string {
  return String(value).padStart(2, "0");
}

function notInItsForm(place: string, noun: string, held: unknown): Stop {
  return new Stop(place + " is " + noun + ", and it holds " + shown(held) + ", so the request is not sent.");
}

function shown(held: unknown): string {
  if (typeof held === "string") return JSON.stringify(held);
  if (Array.isArray(held)) return "a list";
  if (typeof held === "object") return "an object";
  return String(held);
}
