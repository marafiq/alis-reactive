import type { Shape } from "../types/index";
import { RuntimeShape } from "./runtime-shape";

export class RuntimeValue {
  private constructor(
    readonly raw: unknown,
    readonly shape: Shape,
  ) {}

  static declared(raw: unknown, shape: Shape): RuntimeValue {
    return new RuntimeValue(raw, shape);
  }

  usingDeclaredShape(): unknown {
    return applyShapeWhenPresent(this.raw, this.shape);
  }

  usingRequestedShape(shape: Shape): unknown {
    return applyShapeWhenPresent(this.raw, requestedShapeOrDeclared(shape, this.shape));
  }
}

export function isMissingRuntimeValue(value: unknown): boolean {
  return value === null || value === undefined;
}

export function applyShapeWhenPresent(value: unknown, shape: Shape): unknown {
  if (isMissingRuntimeValue(value)) return value;
  return RuntimeShape.from(shape).apply(value);
}

function requestedShapeOrDeclared(requested: Shape, declared: Shape): Shape {
  return RuntimeShape.from(requested).orDeclared(declared);
}
