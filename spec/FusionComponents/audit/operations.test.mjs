import assert from "node:assert/strict";
import {spawnSync} from "node:child_process";
import {existsSync, mkdtempSync, readFileSync, rmSync} from "node:fs";
import {tmpdir} from "node:os";
import {join} from "node:path";
import {fileURLToPath} from "node:url";
import {test} from "node:test";

const cli = fileURLToPath(new URL("./operations.mjs", import.meta.url));

function withAudit(target, check) {
    const folder = mkdtempSync(join(tmpdir(), "alis-operations-"));
    try {
        const output = join(folder, "report.json");
        const result = spawnSync(process.execPath, [cli, target, output], {encoding: "utf8"});
        assert.ifError(result.error);
        check(result, () => JSON.parse(readFileSync(output, "utf8")), output);
    } finally {
        rmSync(folder, {recursive: true});
    }
}

test("a pinned class can be audited before its Fusion slice exists", () => {
    withAudit("@syncfusion/ej2-querybuilder:QueryBuilder", (run, read) => {
        assert.equal(run.status, 0, run.stderr);
        const report = read();
        assert.equal(report.slice, null);
        assert.deepEqual(report.declarations, []);
        const root = report.objects[report.root];
        assert.equal(root.name, "QueryBuilder");
        const returned = root.edges.find(edge => edge.site === "getRules#1.return");
        assert.equal(report.objects[returned.to].name, "RuleModel");
    });
});

test("a type-only export does not qualify as a component", () => {
    withAudit("@syncfusion/ej2-querybuilder:RuleModel", (run, _read, output) => {
        assert.equal(run.status, 1, run.stderr);
        assert.match(run.stderr, /no public component class/);
        assert.equal(existsSync(output), false);
    });
});

test("Grid keeps both call arities and follows document-hidden shapes", () => {
    withAudit("FusionGrid", (run, read) => {
        assert.equal(run.status, 0, run.stderr);
        const report = read();
        const root = report.objects[report.root];
        const range = root.members.find(member => member.name === "selectCellsByRange");
        assert.deepEqual(range.operations.map(call => call.count), [1, 2]);
        const coordinate = root.edges.find(edge => edge.site === "selectCellsByRange#1.arg.startIndex");
        assert.equal(report.objects[coordinate.to].docHidden, true);
        assert.deepEqual(report.objects[coordinate.to].members.map(member => member.name), ["cellIndex", "rowIndex"]);
        const selected = root.edges.find(edge => edge.site === "getSelectedRowCellIndexes#1.return");
        assert.equal(report.objects[selected.to].name, "ISelectedCell");
    });
});

test("ComboBox exposes its filtering call forms and separates live reads from array writes", () => {
    withAudit("FusionComboBox", (run, read) => {
        assert.equal(run.status, 0, run.stderr);
        const report = read();
        const root = report.objects[report.root];
        const filtering = root.edges.find(edge => edge.site === "filtering.payload");
        const update = report.objects[filtering.to].members.find(member => member.name === "updateData");
        assert.deepEqual(update.operations.map(call => call.count), [1, 2, 3]);
        assert.ok(update.operations[0].arityCandidates.length > 0);
        const placement = root.members.find(member => member.name === "dataSource");
        const [get, set] = placement.operations;
        assert.equal(get.kind, "read");
        assert.equal(set.kind, "write");
        const declared = report.declarations.flatMap(type => type.members);
        assert.ok(declared.some(member => get.candidates.includes(member.at) && member.operator === "LiveObject"));
        assert.ok(declared.some(member => set.candidates.includes(member.at) && member.operator === "WriteOnly"));
    });
});
