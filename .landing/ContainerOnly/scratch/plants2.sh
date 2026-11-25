#!/usr/bin/env bash
set -uo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
. scripts/toolchain-env.sh
out=/Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only/.landing/ContainerOnly/scratch
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_47_0 --in Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts --replace $'      appendHidden(form, body.inputs, body.fields.map(field => field.name));' --with $'' > "$out/plant2-x947-outcome.log" 2>&1 < /dev/null
echo "plant2-x947-outcome exit $?"
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_47_0 --in Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts --replace $'  appendInputs(query, send.query.inputs, zone, invariantPlace, request);' --with $'  appendHidden(query, send.query.inputs, []); appendInputs(query, send.query.inputs, zone, invariantPlace, request);' > "$out/plant2-x947-opposite.log" 2>&1 < /dev/null
echo "plant2-x947-opposite exit $?"
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_47_0 --in Alis.Reactive.Assets/runtime/InputsAndIds/page.ts --replace $':not(:disabled)' --with $'' > "$out/plant2-x947-disabled.log" 2>&1 < /dev/null
echo "plant2-x947-disabled exit $?"
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_47_0 --in Alis.Reactive.Assets/runtime/InputsAndIds/page.ts --replace $'return [...container.querySelectorAll' --with $'return [...document.querySelectorAll' > "$out/plant2-x947-outside.log" 2>&1 < /dev/null
echo "plant2-x947-outside exit $?"
caffeinate -i scripts/plant-defect.sh --test M9_F2.M9_X254_0 --in Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts --replace $'      appendHidden(form, body.inputs, body.fields.map(field => field.name));' --with $'' > "$out/plant2-x254-token.log" 2>&1 < /dev/null
echo "plant2-x254-token exit $?"
caffeinate -i scripts/plant-defect.sh --test M9_F2.M9_X254_0 --in tests/Alis.Reactive.Conformance.App/Care/MoveIn/ReceptionDocuments.cshtml --replace $'                .Set(r => r.Kind, Model.Kind)\n' --with $'' > "$out/plant2-x254-documents.log" 2>&1 < /dev/null
echo "plant2-x254-documents exit $?"
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_27_0 --in tests/Alis.Reactive.Conformance.App/Care/Admission/IntakeDocuments.cshtml --replace $'.File(m => m.Documents[1].File, pipeline.Component<IntakeDocumentFile>("second-document-file")' --with $'.File(m => m.Documents[1].File, pipeline.Component<IntakeDocumentFile>("first-document-file")' > "$out/plant2-x927.log" 2>&1 < /dev/null
echo "plant2-x927 exit $?"
caffeinate -i scripts/plant-defect.sh --test M8_F4.X16_18_0 --in tests/Alis.Reactive.Conformance.App/Care/Round/SupplyCodes.cshtml --replace $'body.Set(r => r.Code, pipeline.Component<NativeTextBox>("supply-code").Value)' --with $'body.Set(r => r.Code, "")' > "$out/plant2-x1618.log" 2>&1 < /dev/null
echo "plant2-x1618 exit $?"
caffeinate -i scripts/plant-defect.sh --test Cells.http_beside_hidden --in Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts --replace $'      appendHidden(form, body.inputs, body.parts.map(partName));' --with $'' > "$out/plant2-beside-hidden.log" 2>&1 < /dev/null
echo "plant2-beside-hidden exit $?"
caffeinate -i scripts/plant-defect.sh --test M21_F38.M21_X1038_0 --in Alis.Reactive.Fusion/FusionComponents/FusionRichTextEditor/FusionRichTextEditorHtml.cs --replace $'.HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name })' --with $'' > "$out/plant2-x1038-outcome.log" 2>&1 < /dev/null
echo "plant2-x1038-outcome exit $?"
caffeinate -i scripts/plant-defect.sh --test M21_F38.M21_X1038_0 --in Alis.Reactive.Fusion/FusionComponents/FusionRichTextEditor/FusionRichTextEditorHtml.cs --replace $'JsonValueKind.Null => null,' --with $'JsonValueKind.Null => "<p>No summary.</p>",' > "$out/plant2-x1038-opposite.log" 2>&1 < /dev/null
echo "plant2-x1038-opposite exit $?"
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_15_0 --in Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts --replace $'for (const text of texts) form.append(field.name, text);' --with $'' > "$out/plant2-x915-outcome.log" 2>&1 < /dev/null
echo "plant2-x915-outcome exit $?"
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_15_0 --in Alis.Reactive.Assets/runtime/InputsAndIds/page.ts --replace $'return input !== null && container.contains(input);' --with $'return input !== null;' > "$out/plant2-x915-opposite.log" 2>&1 < /dev/null
echo "plant2-x915-opposite exit $?"
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_15_0 --in Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts --replace $'form.set(writing.token.name, writing.token.value);\n      return form;\n    }\n    case "multipart"' --with $'form.append(writing.token.name, writing.token.value);\n      return form;\n    }\n    case "multipart"' > "$out/plant2-x915-token.log" 2>&1 < /dev/null
echo "plant2-x915-token exit $?"
