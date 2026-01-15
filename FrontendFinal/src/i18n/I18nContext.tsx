import React, { createContext, useContext, useState, useEffect } from 'react';
import { es } from './locales/es';
import { en } from './locales/en';

export type Locale = 'es' | 'en';

const translations = {
    es,
    en
};

interface I18nContextType {
    locale: Locale;
    setLocale: (locale: Locale) => void;
    t: typeof es;
}

const I18nContext = createContext<I18nContextType | undefined>(undefined);

export const I18nProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [locale, setLocaleState] = useState<Locale>(() => {
        // Intentar obtener del localStorage
        const saved = localStorage.getItem('locale');
        if (saved === 'es' || saved === 'en') {
            return saved;
        }

        // Detectar idioma del navegador
        const browserLang = navigator.language.split('-')[0];
        return browserLang === 'es' ? 'es' : 'en';
    });

    const setLocale = (newLocale: Locale) => {
        setLocaleState(newLocale);
        localStorage.setItem('locale', newLocale);
        document.documentElement.lang = newLocale;
    };

    useEffect(() => {
        document.documentElement.lang = locale;
    }, [locale]);

    const value = {
        locale,
        setLocale,
        t: translations[locale]
    };

    return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
};

export const useTranslation = () => {
    const context = useContext(I18nContext);
    if (!context) {
        throw new Error('useTranslation must be used within I18nProvider');
    }
    return context;
};

// Hook simplificado para solo obtener traducciones
export const useT = () => {
    const { t } = useTranslation();
    return t;
};
