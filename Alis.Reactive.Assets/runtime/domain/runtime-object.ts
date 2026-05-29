import type { BrowserObjectContract, MethodArgumentContract, Shape } from "../types";
import { assertNever } from "../core/assert-never";
import { RuntimePath } from "./runtime-path";
import { RuntimeValue } from "./runtime-value";
import { RuntimeShape } from "./runtime-shape";

export class RuntimeObject {
  constructor(
    readonly label: string,
    readonly root: unknown,
    readonly objectContract: BrowserObjectContract,
  ) {}

  read(member: string): RuntimeValue {
    const property = this.objectContract.properties[member]!;
    const label = memberLabel(this.label, member);
    const raw = RuntimePath.from(property.path).readDeclared(this.root, label);
    return RuntimeValue.declared(raw, property.shape);
  }

  set(member: string, value: unknown): void {
    const property = this.objectContract.properties[member]!;
    const label = memberLabel(this.label, member);
    const shaped = RuntimeValue.declared(value, property.shape).usingDeclaredShape();
    RuntimePath.from(property.path).assign(this.root, shaped, label);
  }

  call(member: string, args: unknown[]): RuntimeValue {
    const method = this.objectContract.methods[member]!;
    const label = memberLabel(this.label, member);
    const preparedArgs = prepareMethodArguments(method.arguments, args);
    const raw = RuntimePath.from(method.path).call(this.root, preparedArgs, label);
    return RuntimeValue.declared(raw, method.returns);
  }
}

function memberLabel(objectLabel: string, member: string): string {
  return `${objectLabel}.${member}`;
}

function prepareMethodArguments(contract: MethodArgumentContract, args: unknown[]): unknown[] {
  switch (contract.kind) {
    case "open":
      return args;
    case "exact":
      return prepareExactMethodArguments(contract.shapes, args);
    default:
      assertNever(contract, "method argument contract");
  }
}

function prepareExactMethodArguments(shapes: Shape[], args: unknown[]): unknown[] {
  return args.map((arg, index) => {
    return RuntimeShape.from(shapes[index]!).apply(arg);
  });
}
