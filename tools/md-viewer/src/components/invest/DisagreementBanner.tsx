import { AlertTriangle } from 'lucide-react';

interface DisagreementBannerProps {
  passCount: number;
  failCount: number;
  criterionName: string;
}

export function DisagreementBanner({ passCount, failCount, criterionName }: DisagreementBannerProps) {
  if (!(passCount > 0 && failCount > 0)) return null;

  return (
    <div className="bg-amber-50 border border-amber-200 rounded-md px-3 py-2 flex items-center gap-2 text-sm text-amber-800">
      <AlertTriangle className="w-4 h-4 shrink-0 text-amber-500" />
      <span>
        Agents disagree on {criterionName} &mdash; {passCount} pass, {failCount} fail
      </span>
    </div>
  );
}
