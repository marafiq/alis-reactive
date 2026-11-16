import { assertNever, notBuilt, type Culture, type FlagValue, type ScalarWireForm, type SentField, type TimeZone, type ValueRead } from "../Schema/index";
import { flagNames } from "../Conditions/index";
import { Stop } from "../ValuesAndSources/index";

export interface TextPlace {
  readonly decimal: string;
  readonly dateWithTime: boolean;
}

export const invariantPlace: TextPlace = { decimal: ".", dateWithTime: false };

export function formDataPlace(culture: Culture): TextPlace {
  return { decimal: culture.decimal, dateWithTime: true };
}

export function held(read: ValueRead): unknown {
  switch (read.kind) {
    case "nothing":
      return undefined;
    case "held":
      return read.value;
    default:
      return assertNever(read);
  }
}

export function fieldTexts(field: SentField, zone: TimeZone, place: TextPlace): readonly string[] {
  const value = held(field.value);
  if (value === undefined || value === null) {
    switch (field.whenNothing) {
      case "leave":
        return [];
      case "empty":
        return [""];
      default:
        return assertNever(field.whenNothing);
    }
  }
  const form = field.form;
  switch (form.kind) {
    case "list":
      if (!Array.isArray(value)) throw invalid(field.name, "a list");
      return Array.from(value as readonly unknown[], item => {
        if (item === undefined || item === null) {
          switch (form.itemWhenNothing) {
            case "empty":
              return "";
            case "stop":
              throw invalid(field.name, "a list whose items all hold values");
            default:
              return assertNever(form.itemWhenNothing);
          }
        }
        return scalarText(item, form.item, field.name, zone, place);
      });
    case "text":
    case "number":
    case "boolean":
    case "written":
    case "date":
    case "instant":
    case "enum":
    case "flags":
      return [scalarText(value, form, field.name, zone, place)];
    default:
      return assertNever(form);
  }
}

export function scalarText(value: unknown, form: ScalarWireForm, name: string, zone: TimeZone, place: TextPlace): string {
  switch (form.kind) {
    case "text":
      if (typeof value !== "string") throw invalid(name, "text");
      return value;
    case "number":
      if (typeof value !== "number" || !Number.isFinite(value)) throw invalid(name, "a finite number");
      return String(value).split(".").join(place.decimal);
    case "boolean":
      if (typeof value !== "boolean") throw invalid(name, "a boolean");
      return value ? "true" : "false";
    case "written":
      if (typeof value !== "string" && (typeof value !== "number" || !Number.isFinite(value))) throw invalid(name, "written text or a finite number");
      return String(value);
    case "date": {
      const date = dateValue(value, name);
      return place.dateWithTime ? dayText(date) + timeText(date) : dateText(date);
    }
    case "instant":
      return instantText(dateValue(value, name), zone);
    case "enum": {
      const row = form.values.find(row => row.held === value);
      if (row === undefined) {
        throw new Stop("The request's member '" + name + "' holds " + JSON.stringify(value) + "; its enum forms are " +
          form.values.map(row => JSON.stringify(row.held)).join(", ") + ".");
      }
      return row.name;
    }
    case "flags":
      return flagsText(value, form.values, name);
    default:
      return assertNever(form);
  }
}

function invalid(name: string, expected: string): Stop {
  return new Stop("The request's member '" + name + "' must hold " + expected + ".");
}

function flagsText(value: unknown, rows: readonly FlagValue[], name: string): string {
  const names = flagNames(value, rows);
  if (names === undefined) throw invalidFlags(value, name);
  return names.join(", ");
}

function invalidFlags(value: unknown, name: string): Stop {
  return new Stop("The request's member '" + name + "' holds " + JSON.stringify(value) + ", which its flags rows cannot split.");
}

function dateValue(value: unknown, name: string): Date {
  if (!(value instanceof Date) || !Number.isFinite(value.getTime()) || value.getFullYear() < 1 || value.getFullYear() > 9999) {
    throw invalid(name, "a date in years 1 to 9999");
  }
  return value;
}

function digits(value: number, places = 2): string {
  return String(value).padStart(places, "0");
}

function dayText(value: Date): string {
  return digits(value.getFullYear(), 4) + "-" + digits(value.getMonth() + 1) + "-" + digits(value.getDate());
}

function timeText(value: Date): string {
  const fraction = value.getMilliseconds() === 0 ? "" : "." + digits(value.getMilliseconds(), 3);
  return "T" + digits(value.getHours()) + ":" + digits(value.getMinutes()) + ":" + digits(value.getSeconds()) + fraction;
}

function dateText(value: Date): string {
  const midnight = value.getHours() === 0 && value.getMinutes() === 0 && value.getSeconds() === 0 && value.getMilliseconds() === 0;
  return dayText(value) + (midnight ? "" : timeText(value));
}

function instantText(value: Date, zone: TimeZone): string {
  switch (zone.kind) {
    case "clock":
      return dayText(value) + timeText(value) + offsetText(-value.getTimezoneOffset());
    case "zone":
      return zonedInstant(value, zone.id);
    default:
      return assertNever(zone);
  }
}

function zonedInstant(value: Date, zone: string): string {
  const parts = new Intl.DateTimeFormat("en-US", {
    timeZone: zone, hourCycle: "h23", era: "short", year: "numeric", month: "numeric", day: "numeric", hour: "numeric", minute: "numeric", second: "numeric",
  }).formatToParts(value);
  const part = (type: Intl.DateTimeFormatPartTypes): string => {
    const found = parts.find(part => part.type === type);
    if (found === undefined) throw new Stop("The browser's time zone '" + zone + "' supplied no " + type + ".");
    return found.value;
  };
  const year = Number(part("year"));
  if (part("era") !== "AD" || year > 9999) throw new Stop("The request's instant is outside years 1 to 9999 in time zone '" + zone + "'.");
  const wall = new Date(0);
  wall.setUTCFullYear(year, Number(part("month")) - 1, Number(part("day")));
  wall.setUTCHours(Number(part("hour")), Number(part("minute")), Number(part("second")), value.getMilliseconds());
  const east = (wall.getTime() - value.getTime()) / 60_000;
  const fraction = value.getMilliseconds() === 0 ? "" : "." + digits(value.getMilliseconds(), 3);
  return digits(year, 4) + "-" + part("month").padStart(2, "0") + "-" + part("day").padStart(2, "0") +
    "T" + part("hour").padStart(2, "0") + ":" + part("minute").padStart(2, "0") + ":" + part("second").padStart(2, "0") + fraction + offsetText(east);
}

function offsetText(east: number): string {
  if (east === 0) return "Z";
  if (!Number.isInteger(east)) throw notBuilt("an instant with an offset containing seconds: DateWire states minute offsets");
  const magnitude = Math.abs(east);
  return (east < 0 ? "-" : "+") + digits(Math.floor(magnitude / 60)) + ":" + digits(magnitude % 60);
}
