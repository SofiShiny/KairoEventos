export const parseJwt = (token: string) => {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        return JSON.parse(jsonPayload);
    } catch (e) {
        return null;
    }
};

export const getUserRoles = (user: any): string[] => {
    if (!user) return [];

    // 1. Del profile (ID Token)
    const profile = user.profile as any;
    const realmRolesProfile = profile?.realm_access?.roles || [];

    // 2. Del Access Token (Keycloak suele poner los roles aquí)
    const accessTokenClaims = user.access_token ? parseJwt(user.access_token) : null;
    const realmRolesAccess = accessTokenClaims?.realm_access?.roles || [];
    const resourceRolesAccess = Object.values(accessTokenClaims?.resource_access || {})
        .flatMap((resource: any) => (resource as any).roles || []);

    const rawRoles: string[] = [...new Set([
        ...realmRolesProfile,
        ...realmRolesAccess,
        ...resourceRolesAccess,
        ...(profile?.roles || [])
    ])];

    return rawRoles.map(r => String(r).toLowerCase());
};
