import { readFileSync } from "node:fs";

const catalog = JSON.parse(readFileSync(process.argv[2], "utf8"));
const slice = catalog.slices.FusionQueryBuilder;
const contexts = new Map(slice.contexts.map(context => [context.path, context]));
for (const name of ["ColumnsModel", "Validation"]) {
  const shape = Object.entries(catalog.objects).find(([id, value]) =>
    id.startsWith("@syncfusion/ej2-querybuilder/") && value.name === name);
  contexts.set(name, {id: shape[0]});
}

const reasons = {
  "$.allowValidation": "Builder option used to enable required editor checks",
  "$.columns": "Builder option used for the permitted fields, types and editors",
  "$.dataSource": "Local data supplied by the typed render helper",
  "$.rule": "Initial tree supplied through the pinned EJ2 builder",
  "$.getValidRules": "Typed zero-argument call on the current draft. Explicit source tree is probe-only",
  "$.getFilteredRecords": "Typed asynchronous call with local array rows. Other adaptor envelopes remain unproven",
  "$.getDataManagerQuery": "Probe executes the projected local Query. No screen asks for a Query object",
  "$.getRules": "Probe observes the editing tree including incomplete rules",
  "$.setRules": "Probe imports a vendor tree. The worklist starts its filter through the render helper",
  "class.RuleModel.value": "Heterogeneous scalar, homogeneous array or null. Retained in the vendor tree",
  "class.RuleModel.not": "Group flag preserved by getValidRules and ignored by getPredicate on the pinned build",
  "class.RuleModel.isLocked": "Editor state preserved by getValidRules",
  "class.RuleModel.condition": "Root or group connector, or per-rule connector in separate mode",
  "class.RuleModel.field": "Field of the local application record",
  "class.RuleModel.type": "Editor data type",
  "class.RuleModel.label": "User-facing editor label",
  "class.RuleModel.operator": "Comparison chosen for the field and its data type",
  "ColumnsModel.field": "Configured with the row member's JSON path",
  "ColumnsModel.type": "Configured with the EJ2 editor data type",
  "ColumnsModel.label": "Configured with the coordinator's label",
  "ColumnsModel.validation": "Column editor validation settings. The demo uses EJ2 defaults",
  "Validation.isRequired": "Checked by validateFields when allowValidation is enabled",
  "Validation.min": "Numeric editor bound",
  "Validation.max": "Numeric editor bound"
};

function signature(member) {
  if (!member.signatures.length) return member.declared;
  return member.signatures.map(call => {
    const args = call.parameters.map(p => (p.rest ? "..." : "") + p.name + (p.optional ? "?" : "") + ": " + p.type);
    return "(" + args.join(", ") + ") => " + call.result.type;
  }).join(" | ");
}

console.log(["context", "owner", "name", "kind", "signature", "optional", "defaultNull", "package location", "C# declaration", "verdict"].join("\t"));
for (const [contextName, context] of contexts) {
  const shape = catalog.objects[context.id];
  for (const member of shape.members) {
    const declarations = context.rows?.present[member.name] ?? [];
    const followUp = member.assessment === "DOM boundary" ? "DOM object outside the worklist"
      : member.assessment === "shape needs review" ? "Separate gesture and shape evidence required"
      : "Outside the demonstrated worklist operations";
    const verdict = reasons[contextName + "." + member.name] ?? (declarations.length ? "Exposed and exercised by the worklist" : followUp);
    console.log([contextName, member.owner, member.name, member.access, signature(member), member.optional, member.defaultNull,
      member.at, declarations.join(","), verdict].join("\t"));
  }
}
