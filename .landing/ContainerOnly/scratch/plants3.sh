#!/usr/bin/env bash
set -uo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
. scripts/toolchain-env.sh
out=/Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only/.landing/ContainerOnly/scratch
caffeinate -i scripts/plant-defect.sh --test M9_F2.M9_X254_0 --in tests/Alis.Reactive.Conformance.App/Care/MoveIn/ReceptionDocuments.cshtml --replace $'                .Set(r => r.Caption, pipeline.Component<NativeTextBox>("reception-document-caption").Value)\n' --with $'' > "$out/plant3-x254-caption.log" 2>&1 < /dev/null
echo "plant3-x254-caption exit $?"
caffeinate -i scripts/plant-defect.sh --test M8_F4.X16_18_0 --in tests/Alis.Reactive.Conformance.App/Care/Round/SupplyCodes.cshtml --replace $'body.Set(r => r.Code, pipeline.Component<NativeTextBox>("supply-code").Value)' --with $'body.Set(r => r.Code, "A12")' > "$out/plant3-x1618.log" 2>&1 < /dev/null
echo "plant3-x1618 exit $?"
