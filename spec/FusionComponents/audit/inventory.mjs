import { strict as assert } from "node:assert";
import { createHash } from "node:crypto";
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { shippedSlices } from "./fusion.mjs";
import { vendorCatalog } from "./ej2.mjs";

const [destination, tableFile, mode, ...extra] = process.argv.slice(2);
const verifyBaseline = mode === "--verify-baseline";
if (!destination || !tableFile || (mode && !verifyBaseline) || extra.length) {
    throw Error("Usage: node spec/FusionComponents/audit/inventory.mjs <graph.json> <slice-rows.tsv> [--verify-baseline]");
}
const slices = shippedSlices();
const {versions, compiler, shapes, roots, mirrorSymbols, unresolved} = vendorCatalog(slices);
const vendor = id => shapes[id];
const childName = symbol => symbol.split(".").at(-1);
function heldClass(slice, doors, shape) {
    const matches = slice.types.slice(1).filter(t => doors.some(m => {
        const local = childName(t.symbol);
        const name = local.split("<")[0];
        const generic = local.includes("<");
        const returnType = m.result;
        return new RegExp("\\b" + name + (generic ? "<" : "(?![<\\w])")).test(returnType);
    }));
    const exact = matches.filter(t => t.mirror?.[1] === shape.name);
    if (exact.length === 1) return exact[0];
    return null;
}
const evidence = {};
const columns = ["slice", "site", "vendor declaration", "C# holder", "present by name", "absent typed name", "absent shape needs review", "absent DOM-valued name", "C# only"];
const table = [columns.join("\t")];
for (const slice of slices) {
    const contexts = [];
    const root = roots[slice.name];
    const positions = new Map();
    function add(path, id, holder) {
        if (!id) return;
        if (positions.has(path)) throw Error("Repeated context: " + slice.name + " " + path);
        positions.set(path, id);
        const shape = vendor(id);
        const rows = {present: {}, absentTyped: [], review: [], dom: [], csharpOnly: []};
        for (const member of shape.members) {
            const present = holder?.members.filter(m => m.operator !== "projection" && m.js === member.name) ?? [];
            if (present.length) rows.present[member.name] = present.map(m=>m.at);
            else if (member.assessment === "DOM boundary") rows.dom.push(member.name);
            else if (member.assessment === "shape needs review" || (slice.name === "FusionComboBox" && path === "event.change" && ["itemData","previousItemData"].includes(member.name))) rows.review.push(member.name);
            else rows.absentTyped.push(member.name);
        }
        for (const door of holder?.members ?? []) {
            if (door.operator === "projection" || !shape.members.some(m => m.name === door.js)) rows.csharpOnly.push(door.name);
        }
        contexts.push({path, id, holder: holder?.symbol ?? null, rows});
        table.push([slice.name,path,id,holder?.symbol ?? "",Object.keys(rows.present),rows.absentTyped,rows.review,rows.dom,rows.csharpOnly].join("\t"));
    }
    if (root) {
        add("$", root, slice.types[0]);
        for (const member of vendor(root).members) {
            const doors = slice.types[0].members.filter(m=>m.js === member.name);
            if (member.payload) {
                for (const id of member.payload.objects) {
                    const holder = heldClass(slice, doors, vendor(id));
                    add("event." + member.name + (member.payload.objects.length > 1 ? "." + id : ""), id, holder);
                }
            }
        }
        for (const type of slice.types.slice(1)) {
            const seen = contexts.some(context => context.holder === type.symbol);
            if (!seen && mirrorSymbols.get(type)) add("class." + childName(type.symbol), mirrorSymbols.get(type), type);
        }
    }
    const separate = slice.types.filter(t => !contexts.some(c=>c.holder === t.symbol)).map(t => t.symbol);
    evidence[slice.name] = {root: root ?? null, declarations: slice.types, contexts, separate};
    if (!root) table.push([slice.name,"$", "",slice.types[0].symbol,"","","","",""].join("\t"));
}
if (verifyBaseline) {
    function getMember(slice, path, name) {
        const context = evidence[slice].contexts.find(c=>c.path === path);
        assert.ok(context, slice + " " + path);
        const row = vendor(context.id).members.find(m=>m.name === name);
        assert.ok(row, name);
        return {context, row, matched: !!context.rows.present[name]};
    }
    function checked(slice, path, name, present, owner) {
        const {row, matched} = getMember(slice,path,name);
        assert.equal(matched,present,slice + " " + path + " " + name + " differs from the frozen baseline");
        if (owner) assert.equal(row.owner,owner);
        return row;
    }
    const data = checked("FusionComboBox","$","dataSource",false,"DropDownBase");
    assert.match(data.declared,/DataManager/);
    checked("FusionComboBox","event.change","itemData",false,"SelectEventArgs");
    checked("FusionComboBox","event.change","previousItemData",false,"ChangeEventArgs");
    checked("FusionComboBox","event.change","item",false,"SelectEventArgs");
    checked("FusionComboBox","event.change","previousItem",false,"ChangeEventArgs");
    checked("FusionMultiSelect","$","dataSource",true);
    const alias = getMember("FusionGrid","$","dataSource").context.rows.present.dataSource;
    assert.equal(alias.length,2);
    for (const [slice, present] of [["FusionComboBox",false],["FusionDropDownList",false],["FusionAutoComplete",true],["FusionMultiSelect",true]]) {
        checked(slice,"$","filtering",present);
        checked(slice,"event.filtering","text",present);
        checked(slice,"event.filtering","cancel",false);
        checked(slice,"event.filtering","baseEventArgs",false);
        const method = checked(slice,"event.filtering","updateData",present);
        assert.equal(method.access,"method");
        const [sig] = method.signatures;
        assert.deepEqual(sig.parameters.map(p=>[p.name,p.optional]),[["dataSource",false],["query",true],["fields",true]]);
        assert.match(sig.parameters[0].type,/DataManager/);
        const cs = getMember(slice,"event.filtering","updateData").context.holder;
        if (present) {
            const impl = evidence[slice].declarations.find(t=>t.symbol === cs).members.find(m=>m.js === "updateData");
            assert.match(impl.source,/UpdateData<TItem>\(Arg<IReadOnlyList<TItem>> dataSource\)/);
        }
    }
    assert.ok(!vendor(roots.FusionAutoComplete).members.some(m=>m.name === "text"));
    assert.ok(!vendor(roots.FusionPivotView).members.some(m=>m.name === "currentView"));
    console.log("Verified inherited ComboBox.dataSource/change.itemData, change.previousItemData and both list elements; MultiSelect.dataSource; Grid.dataSource aliases; four filtering payloads and updateData lanes; private AutoComplete.text and hidden PivotView.currentView excluded.");
}
const result = {compiler, versions, unresolvedRoots: unresolved, slices: evidence, objects: shapes};
const json = JSON.stringify(result) + "\n";
const tsv = table.join("\n") + "\n";
for (const [path, content] of [[destination,json],[tableFile,tsv]]) {
    mkdirSync(dirname(path),{recursive:true});
    writeFileSync(path,content);
    console.log(path + " sha256 " + createHash("sha256").update(content).digest("hex"));
}
console.log("Slices " + slices.length + ", EJ2 roots " + Object.values(roots).filter(Boolean).length + ", unresolved EJ2 roots " + unresolved.length + ", C# classes " + slices.reduce((n,s)=>n+s.types.length,0) + ", vendor object types " + Object.keys(shapes).length + ", per-slice contexts " + (table.length-1));
