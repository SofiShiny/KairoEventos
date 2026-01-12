import React from 'react';

interface StatCardProps {
    title: string;
    value: string | number;
    icon: string;
    description?: string;
    trend?: {
        value: string;
        positive: boolean;
    };
}

export const StatCard: React.FC<StatCardProps> = ({ title, value, icon, description, trend }) => {
    return (
        <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6 hover:border-purple-500/50 transition-all duration-300">
            <div className="flex items-start justify-between">
                <div>
                    <p className="text-gray-400 text-sm font-medium">{title}</p>
                    <h3 className="text-2xl font-bold text-white mt-1">{value}</h3>

                    {trend && (
                        <div className={`flex items-center gap-1 mt-2 text-xs font-semibold ${trend.positive ? 'text-green-400' : 'text-red-400'}`}>
                            <span>{trend.positive ? '↑' : '↓'}</span>
                            <span>{trend.value}</span>
                            <span className="text-gray-500 font-normal ml-1">vs ayer</span>
                        </div>
                    )}

                    {description && (
                        <p className="text-gray-500 text-xs mt-2">{description}</p>
                    )}
                </div>
                <div className="p-3 bg-gray-800 rounded-xl text-2xl">
                    {icon}
                </div>
            </div>
        </div>
    );
};
