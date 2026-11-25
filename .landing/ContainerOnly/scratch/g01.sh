cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- ContainerKind/form literals in runtime ts"
git grep -n -E 'ContainerKind|"form"' f2986ea66 -- 'Alis.Reactive.Assets/runtime/*.ts'
echo "--- count"
git grep -c -E 'ContainerKind|"form"' f2986ea66 -- 'Alis.Reactive.Assets/runtime/*.ts'
echo "--- any .cs directly under Alis.Reactive in cwd?"
ls /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only/Alis.Reactive | grep -c -E '[.]cs$'
