/**
 * Validates that all required environment variables are present
 * Throws an error if any required variable is missing
 * @param envVars - Environment variables object (defaults to import.meta.env)
 */
export function validateEnv(envVars: Record<string, unknown> = import.meta.env): void {
  const requiredEnvVars = [
    'VITE_GATEWAY_URL',
    'VITE_KEYCLOAK_URL',
    'VITE_KEYCLOAK_REALM',
    'VITE_KEYCLOAK_CLIENT_ID',
  ];

  const missingVars: string[] = [];

  requiredEnvVars.forEach((varName) => {
    if (!envVars[varName]) {
      missingVars.push(varName);
    }
  });

  if (missingVars.length > 0) {
    const errorMessage = `
❌ Missing required environment variables:
${missingVars.map((v) => `  - ${v}`).join('\n')}

Please ensure all required variables are defined in your .env file.
See .env.example for reference.
    `.trim();

    throw new Error(errorMessage);
  }

  console.log('✅ Environment variables validated successfully');
}

/**
 * Gets environment configuration with type safety
 */
export const env = {
  gatewayUrl: import.meta.env.VITE_GATEWAY_URL as string,
  keycloak: {
    url: import.meta.env.VITE_KEYCLOAK_URL as string,
    realm: import.meta.env.VITE_KEYCLOAK_REALM as string,
    clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID as string,
  },
  app: {
    name: import.meta.env.VITE_APP_NAME as string,
    version: import.meta.env.VITE_APP_VERSION as string,
  },
} as const;
