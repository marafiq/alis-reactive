#!/usr/bin/env bash
set -uo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
. scripts/toolchain-env.sh
out=.landing/ContainerOnly/scratch
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_15_0 --in Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts --replace 'for (const text of texts) form.append(field.name, text);' --with '' > "$out/plant-x915-outcome.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/plant-x915-outcome.log"
caffeinate -i scripts/plant-defect.sh --test M6_F1.X9_15_0 --in Alis.Reactive.Assets/runtime/InputsAndIds/page.ts --replace 'return input !== null && container.contains(input);' --with 'return input !== null;' > "$out/plant-x915-opposite.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/plant-x915-opposite.log"
caffeinate -i scripts/plant-defect.sh --test M21_F38.M21_X1038_0 --in Alis.Reactive.Fusion/FusionComponents/FusionRichTextEditor/FusionRichTextEditorHtml.cs --replace '.HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name })' --with '' > "$out/plant-x1038-outcome.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/plant-x1038-outcome.log"
caffeinate -i scripts/plant-defect.sh --test M21_F38.M21_X1038_0 --in Alis.Reactive.Fusion/FusionComponents/FusionRichTextEditor/FusionRichTextEditorHtml.cs --replace 'JsonValueKind.Null => null,' --with 'JsonValueKind.Null => "<p>No summary.</p>",' > "$out/plant-x1038-opposite.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/plant-x1038-opposite.log"
echo done > "$out/plants.done"
