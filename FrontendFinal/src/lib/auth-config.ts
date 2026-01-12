import { WebStorageStateStore } from 'oidc-client-ts';
import { AuthProviderProps } from 'react-oidc-context';

export const oidcConfig: AuthProviderProps = {
    authority: 'http://localhost:8180/realms/Kairo',
    client_id: 'kairo-web',
    redirect_uri: window.location.origin,
    response_type: 'code',
    scope: 'openid profile email offline_access',
    userStore: new WebStorageStateStore({ store: window.localStorage }),
    onSigninCallback: (_user: any): void => {
        // Limpiar la URL de los parámetros de OIDC (?code=...)
        window.history.replaceState({}, document.title, window.location.pathname);
    },
};
