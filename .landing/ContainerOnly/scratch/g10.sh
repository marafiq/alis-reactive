cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
grep -E '^spec/(InputsAndIds|RequestsAndResponses)/[^/]*[.](cfg|tla)$' .landing/ContainerOnly/scratch/tree.txt > .landing/ContainerOnly/scratch/tla-files.txt
wc -l .landing/ContainerOnly/scratch/tla-files.txt
git show f2986ea66:spec/InputsAndIds/ContainerRead.cfg
echo "--- a violates cfg"
git show f2986ea66:spec/InputsAndIds/ContainerRead.violates.ReadIsWhatTheEndpointBinds.cfg
echo "--- Request.cfg"
git show f2986ea66:spec/RequestsAndResponses/Request.cfg
echo "--- Request violates FailedCheckSendsNothing"
git show f2986ea66:spec/RequestsAndResponses/Request.violates.FailedCheckSendsNothing.cfg
echo "--- java + jar"
ls /opt/homebrew/opt/openjdk/bin/java
