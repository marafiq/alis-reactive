#!/usr/bin/env bash
set -euo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
git show f2986ea66:spec/InputsAndIds/ContainerRead.tla > spec/InputsAndIds/ContainerRead.tla
git show f2986ea66:spec/RequestsAndResponses/Request.tla > spec/RequestsAndResponses/Request.tla
git show f2986ea66:spec/Core/DESIGN.md > spec/Core/DESIGN.md
git show f2986ea66:spec/Core/schema.md > spec/Core/schema.md
git show f2986ea66:spec/CLAUDE.md > spec/CLAUDE.md
git show f2986ea66:spec/NOTES.md > spec/NOTES.md
git show f2986ea66:spec/REWRITE.md > spec/REWRITE.md
git show f2986ea66:spec/Analyzers/module.md > spec/Analyzers/module.md
git show f2986ea66:spec/RequestsAndResponses/module.md > spec/RequestsAndResponses/module.md
git show f2986ea66:spec/TypeKnowledge/module.md > spec/TypeKnowledge/module.md
git show f2986ea66:spec/RequestsAndResponses/features.json > spec/RequestsAndResponses/features.json
git show f2986ea66:spec/Validation/features.json > spec/Validation/features.json
git show f2986ea66:spec/InputsAndIds/features.json > spec/InputsAndIds/features.json
git show f2986ea66:spec/api/HttpSurface/Objects.cs > spec/api/HttpSurface/Objects.cs
git show f2986ea66:spec/api/HttpSurface/Condensed.cs > spec/api/HttpSurface/Condensed.cs
git show f2986ea66:spec/api/HttpSurface/Parts.cs > spec/api/HttpSurface/Parts.cs
git show f2986ea66:spec/api/HttpSurface/Usage.cs > spec/api/HttpSurface/Usage.cs
git show f2986ea66:spec/api/HttpSurface/EveryStage.cs > spec/api/HttpSurface/EveryStage.cs
git show f2986ea66:spec/api/HttpSurface/refused/SecondContainerOnABody.CS1929.cs.txt > spec/api/HttpSurface/refused/SecondContainerOnABody.CS1929.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/SecondContainerOnGet.CS1929.cs.txt > spec/api/HttpSurface/refused/SecondContainerOnGet.CS1929.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/BodyOnNoContent.CS1061.cs.txt > spec/api/HttpSurface/refused/BodyOnNoContent.CS1061.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/ChainedReadsAnotherType.CS1661.CS1678.cs.txt > spec/api/HttpSurface/refused/ChainedReadsAnotherType.CS1661.CS1678.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/NoRetryOnPost.CS0411.cs.txt > spec/api/HttpSurface/refused/NoRetryOnPost.CS0411.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/NoRetryOnPostAfterSuccess.CS0411.cs.txt > spec/api/HttpSurface/refused/NoRetryOnPostAfterSuccess.CS0411.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/SecondSuccessRoute.CS0411.cs.txt > spec/api/HttpSurface/refused/SecondSuccessRoute.CS0411.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/ValidatorOfAnotherType.CS0311.cs.txt > spec/api/HttpSurface/refused/ValidatorOfAnotherType.CS0311.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/VerbAsExtension.CS1501.cs.txt > spec/api/HttpSurface/refused/VerbAsExtension.CS1501.cs.txt
git show f2986ea66:spec/api/plants/HidesReply.F4.cs.txt > spec/api/plants/HidesReply.F4.cs.txt
git show f2986ea66:spec/api/plants/OtherName.F4.cs.txt > spec/api/plants/OtherName.F4.cs.txt
git show f2986ea66:spec/FusionComponents/features.json > spec/FusionComponents/features.json
git show f2986ea66:spec/Reference/native.json > spec/Reference/native.json
git show f2986ea66:spec/Reference/app.json > spec/Reference/app.json
git show f2986ea66:spec/Fluent/sentences.json > spec/Fluent/sentences.json
git show f2986ea66:spec/Reference/move-in.md > spec/Reference/move-in.md
git show f2986ea66:spec/NativeAppLevelObjects/features.json > spec/NativeAppLevelObjects/features.json
git show f2986ea66:spec/plants/tlc.json > spec/plants/tlc.json
rm -f spec/InputsAndIds/ContainerRead.violates.AnUnsentMemberReadsWhatTheEndpointBinds.HandWrittenControlSent.cfg
rm -f spec/InputsAndIds/ContainerRead.violates.ReachAHandWrittenControlLeftUnsent.cfg
rm -f spec/RequestsAndResponses/Request.violates.FailedCheckSendsNothing.ContainerSubmitsNatively.cfg
rm -f spec/api/HttpSurface/refused/InputsWithNoContainer.CS1501.cs.txt
rm -f spec/api/HttpSurface/refused/InputsInJson.CS1929.cs.txt
rm -f spec/api/HttpSurface/refused/PageFormGone.CS1061.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/FormOfContainer.CS1503.cs.txt > spec/api/HttpSurface/refused/FormOfContainer.CS1503.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/FormWithNoForm.CS1501.cs.txt > spec/api/HttpSurface/refused/FormWithNoForm.CS1501.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/FormInJson.CS1929.cs.txt > spec/api/HttpSurface/refused/FormInJson.CS1929.cs.txt
git show f2986ea66:spec/api/HttpSurface/refused/PageFormGone.CS0411.cs.txt > spec/api/HttpSurface/refused/PageFormGone.CS0411.cs.txt
git show f2986ea66:spec/InputsAndIds/ContainerRead.cfg > spec/InputsAndIds/ContainerRead.cfg
rm -f spec/InputsAndIds/ContainerRead.violates.AHiddenInputReachesAFormBodyAlone.HiddenLeftUnsent.cfg
rm -f spec/InputsAndIds/ContainerRead.violates.AHiddenInputReachesAFormBodyAlone.HiddenSentOutsideAFormBody.cfg
rm -f spec/InputsAndIds/ContainerRead.violates.ReachAHiddenInputInAFormBody.cfg
echo restored
