cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- plan.ts Container / ContainerCapture"
git grep -n -E 'ContainerCapture|ContainerKind|interface Container|CapturedContainerCapture|MissingContainerCapture|NoContainerCapture' f2986ea66 -- Alis.Reactive.Assets/runtime/Schema/plan.ts
echo "--- schema.md submits/sends/captured"
git grep -n -E 'submits|sends\b|captured' f2986ea66 -- spec/Core/schema.md
echo "--- facts: implicit / submit / submission / form attribute"
git grep -n -i -E '"id": "[^"]*(implicit-sub|submit|submission|formdata|form-attr|form-owner|fieldset|nested-form)[^"]*"' f2986ea66 -- spec/probes/facts.json
git grep -c -i -E 'implicit submission|requestSubmit|nested form' f2986ea66 -- spec
echo "--- NativeButton render type"
git grep -n -E 'type=|"button"|submit' f2986ea66 -- 'Alis.Reactive.Native/NativeComponents/NativeButton/*.cs'
echo "--- Native API: components (class lines only)"
git grep -n -E '^Alis[.]Reactive[.]Native[.]NativeComponents[.]Native[A-Za-z]+$' f2986ea66 -- Alis.Reactive.Native/PublicAPI/net10.0/PublicAPI.Unshipped.txt
echo "--- core API: Render< doors"
git grep -n -E '[.]Render<' f2986ea66 -- Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt
echo "--- core API: Inputs / Input( / Container doors"
git grep -n -E 'CondensedRequests[.]Inputs|RequestBlocks[.]|RequestContainerBlock|RequestsAndResponses[.]RequestContainer<TModel>($|[.])|Pipeline<TModel>[.]Container<|RequestSteps[.]Validate|Json[(]Alis[.]Reactive[.]RequestsAndResponses[.]RequestContainer|(UrlEncoded|Multipart)[(]Alis[.]Reactive[.]RequestsAndResponses[.]RequestContainer' f2986ea66 -- Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt Alis.Reactive/PublicAPI/net48/PublicAPI.Unshipped.txt
