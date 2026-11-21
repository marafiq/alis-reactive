import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { join, relative } from "node:path";
import ts from "typescript";

const version = "35.2.12";
const cache = "obj/form-design-inventory";
const packages = {
    "ej2-form-builder": "nOxFbeU+KjSNHAGObqYwVtHR0meQhTxrNOMUpjaPnKwDwnDz8L710EKjtpfB0ghgH+Z/rgN00j9+8PYJaIHmmA==",
    "ej2-form-renderer": "3ndwvfaMZ30d2rg4KrfWoiC/fQCwsZHRmQRNWtmlVdpr7itzfHSjXBhhcRFVMXiECkaTDmCPsue3KsVUZArmdw==",
    "ej2-base": "odr5GyIGYHWN00iVL2jMRq+1YtguNDN0vCtAjqthOEAvEwHap/A9QP+n6Qo/mspfkiSF3se9gb5WxW2mV39ArA==",
};
for (const [name, integrity] of Object.entries(packages)) {
    const directory = join(cache, name);
    mkdirSync(directory, { recursive: true });
    const archive = await fetch("https://registry.npmjs.org/@syncfusion/" + name + "/-/" + name + "-" + version + ".tgz");
    assert.equal(archive.status, 200);
    const bytes = Buffer.from(await archive.arrayBuffer());
    assert.equal(createHash("sha512").update(bytes).digest("base64"), integrity);
    const file = join(directory, "source.tgz");
    writeFileSync(file, bytes);
    execFileSync("tar", ["-xzf", file, "-C", directory]);
}
const builder = "ej2-form-builder/package/src/form-builder/form-builder.d.ts";
const renderer = "ej2-form-renderer/package/src/form-renderer/form-renderer/form-renderer.d.ts";
const component = "ej2-base/package/src/component.d.ts";
const base = "ej2-base/package/src/base.d.ts";
const uses = {
    "FormBuilder.schema": "typed initial admission schema; omitted when restoring the device draft",
    "FormBuilder.enablePersistence": "typed start option; the device draft survives navigation",
    "FormBuilder.onFieldPropertyChanged": "AdmissionFormBuilder.OnFieldPropertyChanged; callback-property listener",
    "FormRenderer.schema": "typed staff preview start; server-saved nurse requirement",
    "FormRenderer.submit": "AdmissionFormRenderer.Submit; the sample admission passed",
    "FieldEdit.fieldName": "FieldEdit.FieldName; only the receiving nurse requirement is published",
    "FieldEdit.propertyName": "FieldEdit.PropertyName; discriminates required",
    "FieldEdit.propertyValue": "FieldEdit.RequiredValue; boolean lane after matching required; other property kinds unbound",
    "Submission.data": "Submission.Data; typed arrivalRoute for the staff admission template",
};
function declarations(file, declaration) {
    const source = ts.createSourceFile(file, readFileSync(join(cache, file), "utf8"), ts.ScriptTarget.Latest, true);
    const type = source.statements.find(node => node.name?.text === declaration);
    assert.ok(type?.members, declaration);
    return type.members.filter(member => member.name && !ts.isConstructorDeclaration(member) &&
        !member.modifiers?.some(mod => [ts.SyntaxKind.PrivateKeyword, ts.SyntaxKind.ProtectedKeyword, ts.SyntaxKind.StaticKeyword].includes(mod.kind)) &&
        !ts.getJSDocTags(member).some(tag => ["private", "hidden", "internal"].includes(tag.tagName.text)))
        .map(member => ({
            name: member.name.getText(source),
            signature: member.getText(source).replace(/\s+/g, " "),
            source: "@syncfusion/" + relative(cache, join(cache, file)).replace("/package/", "/") + ":" +
                (1 + source.getLineAndCharacterOfPosition(member.getStart(source)).line),
        }));
}
const rows = ["site\tmember\tsignature\tpackage source\tadmission consumer"];
function add(site, source, owner, seen) {
    const members = declarations(source, owner);
    for (const member of members) {
        if (seen.has(member.name)) continue;
        const verdict = uses[site + "." + member.name] ??
            (member.name === "appendTo" ? "mount helper creates the actual control on its element" :
                owner === "Component" || owner === "Base" ? "inherited vendor infrastructure; no admission view door" :
                    "not exposed by this admission workflow");
        rows.push([site, member.name, member.signature, member.source, verdict].join("\t"));
    }
    return new Set([...seen, ...members.map(member => member.name)]);
}
for (const [name, file] of [["FormBuilder", builder], ["FormRenderer", renderer]]) {
    let seen = new Set();
    for (const [source, owner] of [[file, name], [component, "Component"], [base, "Base"]]) seen = add(name, source, owner, seen);
}
for (const [site, file, name] of [
    ["FieldEdit", builder, "PropertyChangedEventArgs"], ["DragStarted", builder, "FieldDragStartEventArgs"],
    ["DragOver", builder, "FieldDragOverEventArgs"], ["Dropped", builder, "FieldDroppedEventArgs"],
    ["Submission", renderer, "SubmitEventArgs"], ["Change", renderer, "ChangeEventArgs"],
    ["ButtonClick", renderer, "ButtonClickEventArgs"], ["Failure", renderer, "FailureEventArgs"],
]) add(site, file, name, new Set());
process.stdout.write(rows.join("\n") + "\n");
