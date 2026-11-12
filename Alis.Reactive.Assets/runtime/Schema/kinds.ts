export function assertNever(unknownKind: never): never {
  throw new Error("The plan holds a kind this page does not know: " + JSON.stringify(unknownKind));
}

export function notBuilt(what: string): Error {
  return new Error("This page cannot run " + what + " yet: the framework's code for it is not built.");
}
