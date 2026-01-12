import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App.tsx';
import { validateEnv } from '@shared/utils';
import { AppAuthProvider } from './context/AuthContext';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from '@shared/config';

// Validate environment variables on startup
try {
  validateEnv();
} catch (error) {
  console.error(error);
  // Show error in UI instead of crashing silently
  document.getElementById('root')!.innerHTML = `
    <div style="padding: 2rem; font-family: monospace; color: #dc2626;">
      <h1>⚠️ Configuration Error</h1>
      <pre style="background: #fee; padding: 1rem; border-radius: 4px; overflow: auto;">
${error instanceof Error ? error.message : 'Unknown error'}
      </pre>
    </div>
  `;
  throw error;
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <AppAuthProvider>
        <App />
      </AppAuthProvider>
    </QueryClientProvider>
  </StrictMode>
);
