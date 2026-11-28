type Step = { kind: "set"; text: string } | { kind: "call"; name: string };

export function run(step: Step): string {
  switch (step.kind) {
    case "set":
      return step.text;
    default:
      return "";
  }
}
