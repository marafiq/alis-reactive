import { useState } from 'react';
import { Outlet } from '@tanstack/react-router';
import { Sidebar } from '@/components/layout/Sidebar';
import { ReviewPanel } from '@/components/layout/ReviewPanel';
import { ReviewPanelContext } from '@/components/layout/reviewPanelContext';
import type { ParsedReview } from '@/lib/types';

export function Layout() {
  const [reviewPanelData, setReviewPanelData] = useState<ParsedReview | null>(null);

  return (
    <ReviewPanelContext.Provider value={{ reviewPanelData, setReviewPanelData }}>
      <div className="flex h-screen overflow-hidden bg-background text-foreground">
        {/* Left Sidebar */}
        <Sidebar />

        {/* Main Content */}
        <main className="flex-1 overflow-y-auto pb-20">
          <Outlet />
        </main>

        {/* Review Slide-In Panel */}
        <ReviewPanel
          review={reviewPanelData}
          onClose={() => setReviewPanelData(null)}
        />
      </div>
    </ReviewPanelContext.Provider>
  );
}
