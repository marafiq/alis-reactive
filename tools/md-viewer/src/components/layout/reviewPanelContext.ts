import { createContext, useContext } from 'react';
import type { ParsedReview } from '@/lib/types';

interface ReviewPanelContextValue {
  reviewPanelData: ParsedReview | null;
  setReviewPanelData: (review: ParsedReview | null) => void;
}

export const ReviewPanelContext = createContext<ReviewPanelContextValue>({
  reviewPanelData: null,
  setReviewPanelData: () => {},
});

export function useReviewPanel() {
  return useContext(ReviewPanelContext);
}
