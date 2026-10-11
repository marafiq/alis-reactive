import type { MethodArgumentContract, Path, Shape } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { RuntimePath, describePath } from "./runtime-path";
import { applyShapeWhenPresent } from "./runtime-value";
import { RuntimeShape } from "./runtime-shape";

// A located JavaScript object. A read, write or call names the member's path and shape the way the
// member's C# declaration does, so the object carries nothing but its label and its root.
export class RuntimeObject {
  constructor(
    readonly label: string,
    readonly root: unknown,
  ) {}

  read(path: Path, shape: Shape): unknown {
    const raw = RuntimePath.from(path).readDeclared(this.root, memberLabel(this.label, path));
    return applyShapeWhenPresent(raw, shape);
  }

  set(path: Path, shape: Shape, value: unknown): void {
    const delivered = RuntimeShape.from(shape).formatForObject(applyShapeWhenPresent(value, shape));
    RuntimePath.from(path).assign(this.root, delivered, memberLabel(this.label, path));
  }

  call(path: Path, argumentContract: MethodArgumentContract, args: unknown[]): unknown {
    const preparedArgs = prepareMethodArguments(argumentContract, args);
    return RuntimePath.from(path).call(this.root, preparedArgs, memberLabel(this.label, path));
  }
}

function memberLabel(objectLabel: string, path: Path): string {
  return path.length === 0 ? objectLabel : `${objectLabel}.${describePath(path)}`;
}

export function prepareMethodArguments(contract: MethodArgumentContract, args: unknown[]): unknown[] {
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
    const shape = shapes[index]!;
    return RuntimeShape.from(shape).formatForObject(applyShapeWhenPresent(arg, shape));
  });
}
