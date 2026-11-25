cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- submit in runtime (non-test)"
git grep -n -i -E 'submit|preventDefault|keydown|Enter' f2986ea66 -- 'Alis.Reactive.Assets/runtime/*.ts' ':!*.test.ts' | grep -v -E 'Submission|submits|submissionOf|submitted' 
echo "--- core API: Submit / KeyDown / Prevent / ButtonType"
git grep -n -i -E 'submit|keydown|keyup|keypress|PreventDefault|ButtonType' f2986ea66 -- Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt Alis.Reactive.Native/PublicAPI/net10.0/PublicAPI.Unshipped.txt | cut -c1-330
echo "--- Native/Fusion API naming form"
git grep -n -i -E 'form' f2986ea66 -- Alis.Reactive.Native/PublicAPI/net10.0/PublicAPI.Unshipped.txt Alis.Reactive.Fusion/PublicAPI/net10.0/PublicAPI.Unshipped.txt Alis.Reactive.OpenTelemetry/PublicAPI/net10.0/PublicAPI.Unshipped.txt | cut -c1-330
echo "--- counts Native/Fusion/OTel net10 + net48"
git grep -c -i -E 'form' f2986ea66 -- 'Alis.Reactive.Native/PublicAPI/*' 'Alis.Reactive.Fusion/PublicAPI/*' 'Alis.Reactive.OpenTelemetry/PublicAPI/*'
echo "--- tests pressing Enter"
git grep -c -E 'PressAsync\("Enter"\)|Keyboard.PressAsync' f2986ea66 -- tests | head -50
