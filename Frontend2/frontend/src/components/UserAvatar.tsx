import React from 'react';

interface UserAvatarProps {
    name: string;
    size?: 'sm' | 'md' | 'lg' | 'xl';
    className?: string;
}

/**
 * Componente UserAvatar que muestra las iniciales del usuario
 * con un color de fondo basado en el hash del nombre
 */
export const UserAvatar: React.FC<UserAvatarProps> = ({
    name,
    size = 'md',
    className = ''
}) => {
    // Extraer iniciales (primeras letras de cada palabra)
    const getInitials = (fullName: string): string => {
        if (!fullName || fullName.trim() === '') return '?';

        const words = fullName.trim().split(' ').filter(word => word.length > 0);

        if (words.length === 0) return '?';
        if (words.length === 1) return words[0].substring(0, 2).toUpperCase();

        // Tomar primera letra de las primeras dos palabras
        return (words[0][0] + words[1][0]).toUpperCase();
    };

    // Generar color basado en el hash del nombre
    const getColorFromName = (fullName: string): string => {
        // Colores vibrantes pero legibles
        const colors = [
            'bg-gradient-to-br from-purple-500 to-pink-500',
            'bg-gradient-to-br from-blue-500 to-cyan-500',
            'bg-gradient-to-br from-green-500 to-emerald-500',
            'bg-gradient-to-br from-orange-500 to-red-500',
            'bg-gradient-to-br from-indigo-500 to-purple-500',
            'bg-gradient-to-br from-pink-500 to-rose-500',
            'bg-gradient-to-br from-teal-500 to-green-500',
            'bg-gradient-to-br from-amber-500 to-orange-500',
            'bg-gradient-to-br from-violet-500 to-purple-500',
            'bg-gradient-to-br from-cyan-500 to-blue-500',
        ];

        // Hash simple del nombre
        let hash = 0;
        for (let i = 0; i < fullName.length; i++) {
            hash = fullName.charCodeAt(i) + ((hash << 5) - hash);
            hash = hash & hash; // Convert to 32bit integer
        }

        const index = Math.abs(hash) % colors.length;
        return colors[index];
    };

    // Tamaños
    const sizeClasses = {
        sm: 'w-8 h-8 text-xs',
        md: 'w-10 h-10 text-sm',
        lg: 'w-16 h-16 text-xl',
        xl: 'w-24 h-24 text-3xl',
    };

    const initials = getInitials(name);
    const colorClass = getColorFromName(name);

    return (
        <div
            className={`
        ${sizeClasses[size]}
        ${colorClass}
        ${className}
        rounded-full
        flex
        items-center
        justify-center
        text-white
        font-bold
        shadow-lg
        select-none
        transition-transform
        hover:scale-105
      `}
            title={name}
        >
            {initials}
        </div>
    );
};
