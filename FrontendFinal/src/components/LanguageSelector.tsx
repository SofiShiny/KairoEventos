import { Globe } from 'lucide-react';
import { useTranslation, Locale } from '../i18n/I18nContext';

export const LanguageSelector = () => {
    const { locale, setLocale } = useTranslation();

    const languages: { code: Locale; name: string; flag: string }[] = [
        { code: 'es', name: 'Español', flag: '🇪🇸' },
        { code: 'en', name: 'English', flag: '🇺🇸' }
    ];

    return (
        <div className="relative group">
            <button className="flex items-center gap-2 px-4 py-2 bg-neutral-900 border border-neutral-800 rounded-xl hover:bg-neutral-800 transition-all">
                <Globe className="w-4 h-4 text-neutral-400" />
                <span className="text-sm font-bold text-white">
                    {languages.find(l => l.code === locale)?.flag}
                </span>
            </button>

            <div className="absolute right-0 mt-2 w-48 bg-neutral-900 border border-neutral-800 rounded-2xl shadow-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all z-50">
                {languages.map((lang) => (
                    <button
                        key={lang.code}
                        onClick={() => setLocale(lang.code)}
                        className={`w-full flex items-center gap-3 px-4 py-3 hover:bg-neutral-800 transition-all first:rounded-t-2xl last:rounded-b-2xl ${locale === lang.code ? 'bg-neutral-800' : ''
                            }`}
                    >
                        <span className="text-2xl">{lang.flag}</span>
                        <span className={`text-sm font-bold ${locale === lang.code ? 'text-white' : 'text-neutral-400'}`}>
                            {lang.name}
                        </span>
                        {locale === lang.code && (
                            <span className="ml-auto w-2 h-2 bg-green-500 rounded-full" />
                        )}
                    </button>
                ))}
            </div>
        </div>
    );
};
