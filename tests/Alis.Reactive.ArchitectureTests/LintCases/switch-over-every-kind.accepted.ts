type Step = { kind: "set"; text: string } | { kind: "call"; name: string };

function assertNever(value: never): never {
  throw new Error(String(value));
}

export function run(step: Step): string {
  switch (step.kind) {
    case "set":
      return step.text;
    case "call":
      return step.name;
    default:
      return assertNever(step);
  }
}
