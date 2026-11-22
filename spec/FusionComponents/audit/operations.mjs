import {mkdirSync, writeFileSync} from "node:fs";
import {dirname} from "node:path";
import {vendorCatalog} from "./ej2.mjs";
import {shippedSlices} from "./fusion.mjs";

const [target, output, ...extra] = process.argv.slice(2);
const vendor = /^(@syncfusion\/[a-z0-9-]+):([A-Za-z_$][\w$]*)$/.exec(target ?? "");
if ((!vendor && !/^Fusion[A-Za-z0-9]+$/.test(target ?? "")) || !output || output.startsWith("-") || extra.length) {
    throw Error("Usage: node spec/FusionComponents/audit/operations.mjs <FusionSlice|@syncfusion/package:Export> <report.json>");
}

const vendorRoot = vendor ? {target, package: vendor[1], exportName: vendor[2]} : undefined;
const slices = vendorRoot ? [] : shippedSlices(target);
const {versions, compiler, shapes, roots, mirrorSymbols} = vendorCatalog(slices, {operations: true, vendorRoot});
const root = roots[target] ?? null;
if (vendorRoot && !root) throw Error("Vendor package has no public component class: " + target);
const declarations = (slices[0]?.types ?? []).map(type => ({...type, vendorId: mirrorSymbols.get(type) ?? null}));
const holders = new Map();
for (const holder of declarations) {
    if (!holder.vendorId) continue;
    const id = shapes[holder.vendorId].declarationId;
    if (!holders.has(id)) holders.set(id, []);
    holders.get(id).push(holder);
}

const byKind = {
    read: ["Property", "ReadOnly", "LiveObject", "LiveObjects", "JSON"],
    write: ["Property", "WriteOnly", "Callback"],
    event: ["Event"],
    listener: ["Event"],
    call: ["Method", "MethodAsync", "ReturnedObject", "ReturnedObjects", "MethodAt", "CallItself"]
};

function callForms(parameters) {
    const rest = parameters.findIndex(param => param.rest);
    const fixed = rest < 0 ? parameters.length : rest;
    let required = fixed;
    while (required > 0 && parameters[required - 1].optional) required--;
    const forms = [];
    for (let count = required; count <= fixed; count++) {
        forms.push({count, variadic: rest >= 0 && count === fixed});
    }
    return forms;
}

function operations(member, named) {
    const rows = [];
    function add(kind, detail = {}) {
        const candidates = named.filter(candidate => byKind[kind].includes(candidate.operator));
        const row = {kind, ...detail, candidates: candidates.map(c => c.at)};
        if (kind === "call") {
            row.arityCandidates = candidates.filter(c => c.emittedArguments &&
                (detail.variadic ? c.emittedArguments.length >= detail.count : c.emittedArguments.length === detail.count)).map(c => c.at);
            row.unknownArityCandidates = candidates.filter(c => c.emittedArguments == null).map(c => c.at);
        }
        rows.push(row);
    }
    if (["read/write", "read"].includes(member.access)) add("read");
    if (["read/write", "write"].includes(member.access)) add("write");
    if (member.access === "event") add("event");
    for (const [index, signature] of member.signatures.entries()) {
        for (const form of callForms(signature.parameters)) add(member.access === "event" ? "listener" : "call", {signature: index + 1, ...form});
    }
    return rows;
}

const objects = {};
const pending = [];
const paths = new Map();
if (root) {
    paths.set(root, "$");
    pending.push(root);
}
for (const id of pending) {
    const {members, indexes, ...shape} = shapes[id];
    const path = paths.get(id);
    const mirrors = holders.get(shape.declarationId) ?? [];
    const edges = [];
    function follow(site, position) {
        for (const to of position?.objects ?? []) {
            edges.push({site, to});
            if (paths.has(to)) continue;
            const variant = position.objects.length > 1 ? "<" + shapes[to].name + ">" : "";
            paths.set(to, path + "." + site + variant);
            pending.push(to);
        }
    }
    const rows = members.map(member => {
        const named = mirrors.flatMap(holder => holder.members.filter(m => m.js === member.name));
        follow(member.name, member.value);
        follow(member.name + ".setter", member.setter);
        follow(member.name + ".payload", member.payload);
        for (const [index, signature] of member.signatures.entries()) {
            const site = member.name + "#" + (index + 1);
            for (const p of signature.parameters) follow(site + ".arg." + p.name, p);
            follow(site + ".return", signature.result);
        }
        return {...member, nameCandidates: named.map(c => c.at), operations: operations(member, named)};
    });
    const indexRows = indexes.map((entry, index) => {
        follow("[index" + (index + 1) + "]", entry);
        return {...entry, operations: entry.readonly ? ["read"] : ["read", "write"]};
    });
    objects[id] = {...shape, path, candidateHolders: mirrors.map(h => h.symbol), members: rows, indexes: indexRows, edges};
}

const report = {slice: vendorRoot ? null : target, compiler, versions, root, declarations, objects};
if (vendorRoot) report.vendor = target;
mkdirSync(dirname(output), {recursive: true});
writeFileSync(output, JSON.stringify(report) + "\n");
console.log(target + ": " + pending.length + " reachable vendor shapes; " + (root ? "root " + root : "no pinned EJ2 root") + "; " + output);
