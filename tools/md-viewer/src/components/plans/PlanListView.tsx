import { useEffect } from 'react';
import { useNavigate } from '@tanstack/react-router';
import { usePlans } from '@/hooks/queries';

/**
 * /plans route — auto-redirects to the first plan.
 * If no plans exist, shows an empty state.
 */
export function PlanListView() {
  const { data: plans = [], isLoading } = usePlans();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isLoading && plans.length > 0) {
      navigate({ to: '/plans/$planId', params: { planId: plans[0].id }, replace: true });
    }
  }, [isLoading, plans, navigate]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-muted-foreground text-sm">Loading plans...</div>
      </div>
    );
  }

  if (plans.length === 0) {
    return (
      <div className="flex items-center justify-center h-full text-muted-foreground text-sm">
        No plans yet.
      </div>
    );
  }

  return null;
}
