import {
  createRootRoute,
  createRoute,
  createRouter,
  redirect,
  Outlet,
} from '@tanstack/react-router';
import { Layout } from '@/components/layout/Layout';
import { PlanListView } from '@/components/plans/PlanListView';
import { PlanView } from '@/components/plans/PlanView';
import { Board } from '@/components/board/Board';
import { StoryDetail } from '@/components/stories/StoryDetail';
import { KnowledgeHome } from '@/components/knowledge/KnowledgeHome';
import { ConceptDetail } from '@/components/knowledge/ConceptDetail';

// ── Root Route ──

const rootRoute = createRootRoute({
  component: Layout,
});

// ── Index: redirect / → /plans ──

const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/',
  beforeLoad: () => {
    throw redirect({ to: '/plans' });
  },
});

// ── /plans → PlanListView (auto-selects first plan) ──

const plansRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/plans',
  component: PlanListView,
});

// ── /plans/$planId → PlanView ──

const planRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/plans/$planId',
  component: PlanView,
});

// ── /plans/$planId/stories/$storyId → StoryDetail (plan-scoped) ──

const planStoryRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/plans/$planId/stories/$storyId',
  component: StoryDetail,
});

// ── /plans/$planId/board → Board (plan-scoped) ──

const planBoardRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/plans/$planId/board',
  component: Board,
});

// ── /board → Board (kanban, all stories) ──

const boardRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/board',
  component: Board,
});

// ── /stories/$storyId → StoryDetail (global) ──

const storyRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/stories/$storyId',
  component: StoryDetail,
});

// ── /knowledge → KnowledgeHome ──

const knowledgeRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/knowledge',
  component: KnowledgeHome,
});

// ── /knowledge/$concept → ConceptDetail ──

const conceptRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/knowledge/$concept',
  component: ConceptDetail,
});

// ── Route tree ──

const routeTree = rootRoute.addChildren([
  indexRoute,
  plansRoute,
  planRoute,
  planStoryRoute,
  planBoardRoute,
  boardRoute,
  storyRoute,
  knowledgeRoute,
  conceptRoute,
]);

// ── Router ──

export const router = createRouter({ routeTree });

// ── Type registration ──

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}
