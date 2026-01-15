import React from 'react';
import { useTranslation } from 'react-i18next';
import { Button, ButtonGroup } from '@mui/material';

export const LanguageSwitcher: React.FC = () => {
    const { i18n } = useTranslation();

    const changeLanguage = (lng: string) => {
        i18n.changeLanguage(lng);
    };

    const currentLang = i18n.language.split('-')[0]; // Manejar códigos como 'es-ES'

    const buttonStyle = {
        fontWeight: 600,
        fontSize: '0.75rem',
        py: 0.5,
        px: 1.5,
        borderColor: 'rgba(255, 255, 255, 0.3)',
        color: 'white',
        '&:hover': {
            borderColor: 'white',
            backgroundColor: 'rgba(255, 255, 255, 0.1)',
        },
    };

    const activeButtonStyle = {
        ...buttonStyle,
        backgroundColor: 'rgba(255, 255, 255, 0.2)',
        borderColor: 'white',
        '&:hover': {
            backgroundColor: 'rgba(255, 255, 255, 0.3)',
        }
    };

    return (
        <ButtonGroup
            size="small"
            variant="outlined"
            aria-label="language switcher"
            sx={{ ml: 1, mr: 1 }}
        >
            <Button
                onClick={() => changeLanguage('es')}
                sx={currentLang === 'es' ? activeButtonStyle : buttonStyle}
            >
                ES
            </Button>
            <Button
                onClick={() => changeLanguage('en')}
                sx={currentLang === 'en' ? activeButtonStyle : buttonStyle}
            >
                EN
            </Button>
        </ButtonGroup>
    );
};
