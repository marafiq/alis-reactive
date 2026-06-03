import type { Shape, ValueExpression } from "../types/index";
import { applyShape, convertByShape, type ConvertResult } from "../shared/shape-convert";

const unshapedPlanShape: Shape = { kind: "none" };

export class RuntimeShape {
  private constructor(private readonly shape: Shape) {}

  static from(shape: Shape): RuntimeShape {
    return new RuntimeShape(shape);
  }

  static unshaped(): RuntimeShape {
    return new RuntimeShape(unshapedPlanShape);
  }

  static declaredBy(producer: ValueExpression): RuntimeShape {
    return RuntimeShape.from(producer.shape);
  }

  get planShape(): Shape {
    return this.shape;
  }

  get isDeclared(): boolean {
    return this.shape.kind !== "none";
  }

  item(): RuntimeShape {
    const shapeDescribesArrayItems = this.shape.kind === "array";
    if (shapeDescribesArrayItems) return RuntimeShape.from(this.shape.item);

    return RuntimeShape.unshaped();
  }

  orDeclared(declared: Shape): Shape {
    if (this.isDeclared) return this.shape;

    return declared;
  }

  apply(value: unknown): unknown {
    if (!this.isDeclared) return value;

    return applyShape(value, this.shape);
  }

  applyEach(items: unknown[]): unknown[] {
    if (!this.isDeclared) return items;

    return items.map(item => applyShape(item, this.shape));
  }

  convert(value: unknown): ConvertResult<unknown> {
    return convertByShape(value, this.shape);
  }

  formatForWire(value: unknown): unknown {
    if (!this.isDeclared) return value;

    if (this.shape.kind === "nullable") {
      return RuntimeShape.from(this.shape.inner).formatForWire(value);
    }

    const valueIsDateTimestamp =
      this.shape.kind === "date"
      && typeof value === "number"
      && !Number.isNaN(value);
    if (valueIsDateTimestamp) return new Date(value).toISOString();

    return value;
  }
}
