# Kairo Frontend Final

Arquitectura **Feature-Based** escalable diseñada para un ecosistema de microservicios masivo.

## Estructura de Directorios

- `src/features/`: Un directorio por cada dominio del backend. Contiene componentes de página, lógica de negocio local, hooks específicos y servicios de API del módulo.
- `src/components/ui/`: Componentes base (Botones, Inputs) compartidos.
- `src/components/layout/`: Estructuras de página (Header, Footer, Sidebar).
- `src/lib/`: Configuraciones de librerías externas (axios, i18n).
- `src/hooks/`: Custom hooks de utilidad global.
- `src/locales/`: Archivos de traducción.

## Stack Tecnológico

- **Framework:** React 18 + TypeScript + Vite
- **Estilos:** Tailwind CSS
- **Routing:** React Router DOM (Data Router API)
- **Cliente HTTP:** Axios (con interceptores para JWT)
- **Internacionalización:** i18next

## Guía de Desarrollo

Para agregar una nueva funcionalidad:
1. Crea el directorio en `src/features/[nombre-feature]`.
2. Define tus subcarpetas `components/`, `hooks/`, `services/`.
3. Registra la ruta en `src/router.tsx`.
