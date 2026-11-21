import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { createServer } from "node:http";
import { createRequire } from "node:module";
import { homedir } from "node:os";
import { join, resolve } from "node:path";

const lock = JSON.parse(readFileSync("spec/FusionComponents/audit/form-design-packages.json", "utf8"));
const cache = process.env.NUGET_PACKAGES ?? join(homedir(), ".nuget/packages");
const declared = readFileSync("tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/FormDesignAssets.props", "utf8");
assert.equal(declared.match(/<AdmissionFormAssetVersion>([^<]+)/)[1], lock.version);
assert.deepEqual([...declared.matchAll(/<PackageDownload Include="([^"]+)"/g)].map(match => match[1]), Object.keys(lock.sha512));
for (const [name, hash] of Object.entries(lock.sha512)) {
    const id = name.toLowerCase();
    const bytes = readFileSync(join(cache, id, lock.version, id + "." + lock.version + ".nupkg"));
    assert.equal(createHash("sha512").update(bytes).digest("base64"), hash, name);
}
const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE ?? resolve("tests/Alis.Reactive.Conformance.PlaywrightTests/bin/Release/net10.0/.playwright/package"));
const assets = new Set();
const server = createServer((request, response) => {
    if (request.url === "/") {
        response.setHeader("content-type", "text/html");
        response.end('<!doctype html><html lang="en"><meta charset="utf-8"><link rel="icon" href="data:,"><link rel="stylesheet" href="/_content/Syncfusion.AspNetCore.Themes/styles/bootstrap5.3.css"><script src="/_content/Syncfusion.AspNetCore.FormBuilder/scripts/sf-form-builder.min.js"></script><main><div id="builder"></div><div id="staff"></div></main></html>');
        return;
    }
    const match = /^\/_content\/(Syncfusion\.AspNetCore\.[A-Za-z]+)\/((scripts|styles)\/[A-Za-z0-9.-]+)$/.exec(request.url);
    if (!match || !Object.hasOwn(lock.sha512, match[1])) {
        response.writeHead(404).end();
        return;
    }
    const bytes = readFileSync(join(cache, match[1].toLowerCase(), lock.version, "staticwebassets", match[2]));
    response.setHeader("content-type", match[3] === "scripts" ? "application/javascript" : "text/css");
    assets.add(match[2]);
    response.end(bytes);
});
const schema = {
    version: "0.1.0",
    properties: {
        arrivalRoute: { id: "route-field", name: "arrivalRoute", type: "string", widget: "textbox", label: "Arrival route", defaultValue: "Hospital" },
        receivingNurse: { id: "handover-field", name: "handover", type: "string", widget: "textbox", label: "Receiving nurse", required: false, validationMessage: "Enter the receiving nurse." },
        transferNotes: { id: "notes-field", name: "transferNotes", type: "string", widget: "textbox", label: "Transfer notes", conditions: { visibleWhen: { condition: "and", rules: [{ label: "Arrival route", field: "route-field", operator: "equal", type: "string", value: "Hospital" }] } } },
        submit: { id: "submit-field", name: "submitIntake", type: "button", widget: "button", label: "Submit admission", buttonType: "submit" }
    },
    layout: ["arrivalRoute", "receivingNurse", "transferNotes", "submit"].map(propertyId => ({ type: "field", propertyId })),
    settings: { name: "Admission handover" }
};
await new Promise(resolve => server.listen(0, "127.0.0.1", resolve));
const browser = await chromium.launch();
try {
    const page = await browser.newPage({ viewport: { width: 1450, height: 950 } });
    page.setDefaultTimeout(6500);
    const errors = [];
    page.on("pageerror", error => errors.push(error.message));
    const origin = "http://127.0.0.1:" + server.address().port;
    await page.goto(origin);
    await page.evaluate(initial => {
        const builder = new ej.formbuilder.FormBuilder({ schema: initial, enablePersistence: true });
        builder.appendTo("#builder");
        window.events = [];
        window.heard = e => window.events.push(e);
        builder.addEventListener("onFieldPropertyChanged", window.heard);
    }, schema);
    await page.getByRole("group", { name: "Receiving nurse textbox" }).press("Enter");
    await page.getByRole("tab", { name: "Validation" }).click();
    const required = page.getByRole("checkbox", { name: "Required checkbox" });
    await required.press("Space");
    const ordinaryListener = await page.evaluate(() => window.events.length);
    assert.equal(ordinaryListener, 0);
    await page.evaluate(() => {
        const builder = document.getElementById("builder").ej2_instances[0];
        builder.removeEventListener("onFieldPropertyChanged", window.heard);
        builder.onFieldPropertyChanged = window.heard;
    });
    await required.press("Space");
    await required.press("Space");
    await page.getByRole("group", { name: "Transfer notes textbox" }).press("Enter");
    await page.getByRole("tab", { name: "Conditions" }).click();
    await page.getByRole("button", { name: "Edit Conditional Visibility" }).click();
    await page.getByRole("textbox", { name: "Enter Value" }).fill("Planned");
    await page.getByRole("textbox", { name: "Enter Value" }).press("Tab");
    await page.getByRole("button", { name: "Save", exact: true }).click();
    await page.getByRole("dialog", { name: "Conditions" }).waitFor({ state: "hidden" });
    const changes = await page.evaluate(() => window.events);
    assert.deepEqual(changes.map(e => [e.propertyName, e.fieldName, e.propertyValue]), [
        ["required", "handover", false], ["required", "handover", true]
    ]);
    const imported = await page.evaluate(() => document.getElementById("builder").ej2_instances[0].schema);
    assert.equal(imported.properties.receivingNurse.required, false);
    assert.equal(imported.properties.transferNotes.conditions.visibleWhen.rules[0].value, "Hospital");
    const [download] = await Promise.all([
        page.waitForEvent("download"),
        page.getByRole("button", { name: "Export json schema" }).press("Enter")
    ]);
    const exported = JSON.parse(readFileSync(await download.path(), "utf8"));
    const nurse = Object.values(exported.properties).find(field => field.name === "handover");
    const notes = Object.values(exported.properties).find(field => field.name === "transferNotes");
    assert.equal(nurse.required, true);
    assert.equal(notes.conditions.visibleWhen.rules[0].value, "Planned");
    assert.equal(notes.conditions.visibleWhen.rules[0].field, "route-field");

    await page.reload();
    const reopenedInputSchema = await page.evaluate(() => {
        const builder = new ej.formbuilder.FormBuilder({ enablePersistence: true });
        builder.appendTo("#builder");
        return builder.schema;
    });
    assert.equal(reopenedInputSchema, null);
    await page.getByRole("button", { name: "Preview form", exact: true }).press("Enter");
    const route = page.getByRole("textbox", { name: "Arrival route textbox" });
    const transfer = page.getByRole("textbox", { name: "Transfer notes textbox" });
    assert.equal(await route.inputValue(), "Hospital");
    await transfer.waitFor({ state: "hidden" });
    await route.fill("Planned");
    await route.press("Tab");
    await transfer.waitFor({ state: "visible" });
    await route.fill("Respite");
    await route.press("Tab");
    await transfer.waitFor({ state: "hidden" });

    await page.goto(origin);
    await page.evaluate(initial => {
        initial.properties.receivingNurse.required = true;
        const renderer = new ej.formrenderer.FormRenderer({ schema: initial });
        renderer.appendTo("#staff");
        window.submits = [];
        renderer.submit = e => window.submits.push(e);
    }, schema);
    const submit = page.getByRole("button", { name: "Submit admission" });
    await submit.click();
    await page.getByText("Enter the receiving nurse.", { exact: true }).waitFor();
    const afterInvalid = await page.evaluate(() => window.submits.length);
    assert.equal(afterInvalid, 0);
    await page.getByRole("textbox", { name: "Receiving nurse textbox" }).fill("Elena Ruiz");
    await submit.click();
    const afterValid = await page.evaluate(() => window.submits[0]);
    assert.equal(afterValid.isValid, true);
    assert.equal(afterValid.data.handover, "Elena Ruiz");
    assert.equal(afterValid.data.arrivalRoute, "Hospital");
    await transfer.fill("Oxygen handover");
    await transfer.press("Tab");
    await route.fill("Respite");
    await route.press("Tab");
    await transfer.waitFor({ state: "hidden" });
    await submit.click();
    const afterHiddenAnswer = await page.evaluate(() => window.submits.length);
    assert.equal(afterHiddenAnswer, 1);
    await page.goto(origin);
    await page.evaluate(initial => {
        initial.properties.receivingNurse.required = true;
        initial.properties.receivingNurse.conditions = initial.properties.transferNotes.conditions;
        const renderer = new ej.formrenderer.FormRenderer({ schema: initial });
        renderer.appendTo("#staff");
        window.submits = [];
        renderer.submit = e => window.submits.push(e);
    }, schema);
    const visibleNurse = page.getByRole("textbox", { name: "Receiving nurse textbox" });
    await visibleNurse.waitFor({ state: "visible" });
    assert.equal(await visibleNurse.inputValue(), "");
    assert.equal(await route.inputValue(), "Hospital");
    await submit.click();
    const conditionalRequiredBlank = await page.evaluate(() => window.submits[0]);
    assert.equal(conditionalRequiredBlank.isValid, true);
    assert.deepEqual(errors, []);
    console.log(JSON.stringify({
        version: lock.version, verifiedNuGetPackages: Object.keys(lock.sha512).length,
        officialScriptsServed: [...assets].filter(asset => asset.startsWith("scripts/")).length,
        ordinaryListener, changes, importedNurseRequired: imported.properties.receivingNurse.required,
        importedPredicate: imported.properties.transferNotes.conditions.visibleWhen,
        exportedSchemaVersion: exported.version, exportedNurseRequired: nurse.required,
        exportedPredicate: notes.conditions.visibleWhen, reopenedInputSchema,
        reopenedNotes: { Hospital: "hidden", Planned: "visible", Respite: "hidden" },
        submissionsAfterInvalid: afterInvalid, validSubmit: afterValid,
        submissionsAfterHidingAnAnsweredField: afterHiddenAnswer, conditionalRequiredBlank, pageErrors: errors
    }, null, 2));
} finally {
    await browser.close();
    await new Promise(resolve => server.close(resolve));
}
