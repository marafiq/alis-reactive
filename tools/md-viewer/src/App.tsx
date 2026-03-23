import { useState, createContext, useContext, type ReactNode } from 'react';
import { usePlans, useStories, useConcepts } from '@/hooks/queries';
import { useWebSocket } from '@/hooks/useWebSocket';
import { Sidebar } from '@/components/layout/Sidebar';
import { ReviewPanel } from '@/components/layout/ReviewPanel';
import { VerdictBar } from '@/components/layout/VerdictBar';
import { PlanView } from '@/components/plans/PlanView';
import { Board } from '@/components/board/Board';
import { StoryDetail } from '@/components/stories/StoryDetail';
import { KnowledgeHome } from '@/components/knowledge/KnowledgeHome';
import { ConceptDetail } from '@/components/knowledge/ConceptDetail';
import type { ParsedReview } from '@/lib/types';

// ── Views ──

export type View = 'plans' | 'board' | 'knowledge' | 'files';

// ── WebSocket Context ──

interface WSContext {
  agentProgress: Record<string, { status: string; verdict?: string }>;
  resetProgress: () => void;
}

const WebSocketContext = createContext<WSContext>({
  agentProgress: {},
  resetProgress: () => {},
});

export function useWS() {
  return useContext(WebSocketContext);
}

function WebSocketProvider({ children }: { children: ReactNode }) {
  const ws = useWebSocket();
  return (
    <WebSocketContext.Provider value={ws}>
      {children}
    </WebSocketContext.Provider>
  );
}

// ── App ──

export function App() {
  return (
    <WebSocketProvider>
      <AppShell />
    </WebSocketProvider>
  );
}

function AppShell() {
  // ── View routing state ──
  const [view, setView] = useState<View>('plans');
  const [selectedPlanId, setSelectedPlanId] = useState<string | null>(null);
  const [selectedStoryId, setSelectedStoryId] = useState<string | null>(null);
  const [selectedConceptName, setSelectedConceptName] = useState<string | null>(null);
  const [reviewPanelData, setReviewPanelData] = useState<ParsedReview | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  // ── Data queries ──
  const { data: plans = [] } = usePlans();
  const { data: stories = [] } = useStories();
  const { data: concepts = [] } = useConcepts();

  // ── Selection handlers ──
  function handleSelect(id: string) {
    if (view === 'plans') setSelectedPlanId(id);
    else if (view === 'board') setSelectedStoryId(id);
    else if (view === 'knowledge') setSelectedConceptName(id);
  }

  function handleSwitchView(next: View) {
    setView(next);
    setReviewPanelData(null);
  }

  const currentSelectedId =
    view === 'plans' ? selectedPlanId
    : view === 'board' ? selectedStoryId
    : view === 'knowledge' ? selectedConceptName
    : null;

  return (
    <div className="flex h-screen overflow-hidden bg-background text-foreground">
      {/* Left Sidebar */}
      <Sidebar
        view={view}
        plans={plans}
        stories={stories}
        concepts={concepts}
        selectedId={currentSelectedId}
        searchQuery={searchQuery}
        onSwitchView={handleSwitchView}
        onSelect={handleSelect}
        onSearch={setSearchQuery}
      />

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto pb-20">
        {view === 'plans' && selectedPlanId && (
          <div className="p-8 max-w-5xl">
            <PlanView
              planId={selectedPlanId}
              onSelectStory={(id) => { setView('board'); setSelectedStoryId(id); }}
            />
          </div>
        )}
        {view === 'plans' && !selectedPlanId && plans.length > 0 && (() => { setSelectedPlanId(plans[0].id); return null; })()}
        {view === 'plans' && !selectedPlanId && plans.length === 0 && (
          <div className="flex items-center justify-center h-full text-muted-foreground text-sm">No plans yet.</div>
        )}

        {view === 'board' && !selectedStoryId && (
          <div className="p-8">
            <Board onSelectStory={setSelectedStoryId} />
          </div>
        )}
        {view === 'board' && selectedStoryId && (
          <div className="p-8 max-w-5xl">
            <StoryDetail
              storyId={selectedStoryId}
              onSelectStory={(id) => setSelectedStoryId(id)}
              onSelectConcept={(name) => { setView('knowledge'); setSelectedConceptName(name); }}
              onOpenReview={setReviewPanelData}
            />
          </div>
        )}

        {view === 'knowledge' && !selectedConceptName && (
          <div className="p-8">
            <KnowledgeHome onSelectConcept={setSelectedConceptName} />
          </div>
        )}
        {view === 'knowledge' && selectedConceptName && (
          <div className="p-8 max-w-4xl">
            <ConceptDetail
              conceptName={selectedConceptName}
              onBack={() => setSelectedConceptName(null)}
              onNavigate={(type, id) => {
                if (type === 'plan') { setView('plans'); setSelectedPlanId(id); }
                else if (type === 'story') { setView('board'); setSelectedStoryId(id); }
              }}
            />
          </div>
        )}

        {view === 'files' && (
          <div className="flex items-center justify-center h-full text-muted-foreground text-sm">
            File browser coming soon.
          </div>
        )}
      </main>

      {/* Review Slide-In Panel */}
      <ReviewPanel
        review={reviewPanelData}
        onClose={() => setReviewPanelData(null)}
      />
    </div>
  );
}
