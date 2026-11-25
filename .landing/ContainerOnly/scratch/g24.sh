cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
git grep -c -i -E 'a form sends|with the form|the form[^a-z]|a form[^a-z]|native form' f2986ea66 -- 'Alis.Reactive.Fusion/*.cs' > .landing/ContainerOnly/scratch/fusion-cs-form-counts.txt
wc -l .landing/ContainerOnly/scratch/fusion-cs-form-counts.txt
git grep -n -i -E 'a form sends|with the form|the form[^a-z]|a form[^a-z]|native form' f2986ea66 -- 'Alis.Reactive.Fusion/*.cs' > .landing/ContainerOnly/scratch/fusion-cs-form-lines.txt
wc -l .landing/ContainerOnly/scratch/fusion-cs-form-lines.txt
grep -c -E 'a form sends' .landing/ContainerOnly/scratch/fusion-cs-form-lines.txt
