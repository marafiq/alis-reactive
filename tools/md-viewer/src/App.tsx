import { createContext, useContext, type ReactNode } from 'react';
import { RouterProvider } from '@tanstack/react-router';
import { useWebSocket } from '@/hooks/useWebSocket';
import { router } from '@/router';

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
      <RouterProvider router={router} />
    </WebSocketProvider>
  );
}
