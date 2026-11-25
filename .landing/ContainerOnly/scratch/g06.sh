cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
echo "--- validators of the hand-written pages"
git grep -n -E 'ReactiveValidator<(ReceptionPage|SupplyCodes|IntakeAdmission|BatchNotes|DoseSave|DoseIntake|IntakeDocuments|DocumentsPage)>' f2986ea66 -- tests
echo "--- move-in.md on Reception / submission / hand-written"
git grep -n -i -E 'submission set|hand-written|BeginRequestForm|RequestForm|native form|Reception[.]cshtml' f2986ea66 -- spec/Reference/move-in.md
