import { useEffect, useRef, useCallback, useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';

interface WSMessage {
  type: string;
  storyId?: string;
  role?: string;
  status?: string;
  verdict?: string;
  error?: string;
  completed?: string[];
  failed?: { role: string; error: string }[];
  [key: string]: unknown;
}

export function useWebSocket() {
  const qc = useQueryClient();
  const wsRef = useRef<WebSocket | null>(null);
  const retriesRef = useRef(0);
  const [agentProgress, setAgentProgress] = useState<Record<string, { status: string; verdict?: string }>>({});

  const connect = useCallback(() => {
    const ws = new WebSocket(`ws://${location.host}`);
    wsRef.current = ws;

    ws.onopen = () => { retriesRef.current = 0; };

    ws.onmessage = (e) => {
      let msg: WSMessage;
      try { msg = JSON.parse(e.data); }
      catch { console.error('Malformed WS message:', e.data); return; }

      if (msg.type === 'review-progress' && msg.role) {
        setAgentProgress(prev => ({
          ...prev,
          [msg.role!]: { status: msg.status || 'unknown', verdict: msg.verdict as string },
        }));
      }

      if (msg.type === 'review-complete' && msg.storyId) {
        qc.invalidateQueries({ queryKey: ['reviews', msg.storyId] });
        qc.invalidateQueries({ queryKey: ['stories'] });
        setAgentProgress({});
      }

      if (msg.type === 'review-error' && msg.storyId) {
        console.error(`Review failed for ${msg.storyId}:`, msg.error);
        setAgentProgress({});
      }
    };

    ws.onerror = () => {};
    ws.onclose = () => {
      retriesRef.current++;
      const delay = Math.min(2000 * Math.pow(2, retriesRef.current), 30000);
      setTimeout(connect, delay);
    };
  }, [qc]);

  useEffect(() => { connect(); return () => wsRef.current?.close(); }, [connect]);

  const resetProgress = useCallback(() => setAgentProgress({}), []);

  return { agentProgress, resetProgress };
}
