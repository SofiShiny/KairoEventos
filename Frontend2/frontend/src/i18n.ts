import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

import esTranslation from './locales/es.json';
import enTranslation from './locales/en.json';

i18n
    // Detectar el idioma del navegador
    .use(LanguageDetector)
    // Pasar la instancia de i18n a react-i18next
    .use(initReactI18next)
    // Inicializar i18next
    .init({
        resources: {
            es: {
                translation: esTranslation
            },
            en: {
                translation: enTranslation
            }
        },
        fallbackLng: 'es', // Idioma por defecto
        debug: false,
        interpolation: {
            escapeValue: false // No es necesario para React (previene XSS por defecto)
        },
        detection: {
            order: ['localStorage', 'navigator'],
            caches: ['localStorage']
        }
    });

export default i18n;
