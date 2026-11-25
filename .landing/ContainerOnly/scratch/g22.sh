cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
git grep -n -E '(^|[^a-zA-Z-])(native )?forms?( |,|[.]|:|;|[)])' f2986ea66 -- spec/FusionComponents/features.json > .landing/ContainerOnly/scratch/fusion-features-form.txt
wc -l .landing/ContainerOnly/scratch/fusion-features-form.txt
