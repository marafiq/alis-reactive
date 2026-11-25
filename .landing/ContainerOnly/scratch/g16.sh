cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
git grep -n -i -E 'native form|the form[^a-z]|a form[^a-z]|form.s |submission set|form control' f2986ea66 -- 'Alis.Reactive/*.cs' 'Alis.Reactive.Native/*.cs' 'Alis.Reactive.Analyzers/*.cs' > .landing/ContainerOnly/scratch/cs-form-prose.txt
wc -l .landing/ContainerOnly/scratch/cs-form-prose.txt
git grep -n -i -E 'native form|the form[^a-z]|a form[^a-z]|form.s |submission set|form control' f2986ea66 -- 'Alis.Reactive.Assets/runtime/*.ts' ':!*.test.ts' > .landing/ContainerOnly/scratch/ts-form-prose.txt
wc -l .landing/ContainerOnly/scratch/ts-form-prose.txt
git grep -n -E 'README|BeginRequest|[.]Form[(]|Inputs[(]' f2986ea66 -- README.md 'tests/Alis.Reactive.Consumer.Net10/*.cshtml' 'tests/Alis.Reactive.Consumer.Net48/*.cshtml' > .landing/ContainerOnly/scratch/readme-doors.txt
wc -l .landing/ContainerOnly/scratch/readme-doors.txt
git grep -c -E 'BeginRequestForm|[.]Form[(]|RequestForm' f2986ea66 -- README.md 'tests/Alis.Reactive.Consumer.Net10/*' 'tests/Alis.Reactive.Consumer.Net48/*' 'tests/Alis.Reactive.Poc.App/*' 'poc/*'
