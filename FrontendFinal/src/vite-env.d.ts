/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_USUARIOS_API_URL: string
    readonly VITE_ASIENTOS_API_URL: string
    readonly VITE_EVENTOS_API_URL: string
    readonly VITE_GATEWAY_API_URL: string
    readonly VITE_PAGOS_API_URL: string
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}
