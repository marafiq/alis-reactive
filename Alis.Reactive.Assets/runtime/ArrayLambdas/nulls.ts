import { nothing, Stop, type Read } from "../ValuesAndSources/index";

export function nonNull(field: Read): Read {
  if (field.kind === "nothing") throw new Stop("A list's field reads nothing, and its type holds no null.");
  if (field.value === null) throw new Stop("A list's field holds null, and its type holds no null.");
  return field;
}

export function nullableValue(nullable: Read): Read {
  return nullable.kind === "value" && nullable.value === null ? nothing : nullable;
}
