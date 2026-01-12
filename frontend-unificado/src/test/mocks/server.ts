import { setupServer } from 'msw/node';
import { handlers } from './handlers';

/**
 * MSW Server for Node environment (tests)
 * This server intercepts HTTP requests during tests
 */
export const server = setupServer(...handlers);
