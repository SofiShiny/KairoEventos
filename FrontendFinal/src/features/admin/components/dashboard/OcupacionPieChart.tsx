import React from 'react';
import {
    PieChart,
    Pie,
    Cell,
    ResponsiveContainer,
    Tooltip,
    Legend
} from 'recharts';

interface OcupacionPieChartProps {
    data: Array<{
        nombre: string;
        vendidas: number;
        disponibles: number;
    }>;
}

export const OcupacionPieChart: React.FC<OcupacionPieChartProps> = ({ data }) => {
    // Si hay múltiples eventos, sumamos para el gráfico general o mostramos el primero
    const totalVendidas = data.reduce((sum, item) => sum + item.vendidas, 0);
    const totalDisponibles = data.reduce((sum, item) => sum + item.disponibles, 0);

    const chartData = [
        { name: 'Vendidas', value: totalVendidas },
        { name: 'Disponibles', value: totalDisponibles }
    ];

    const COLORS = ['#ec4899', '#374151'];

    return (
        <div className="h-[300px] w-full">
            <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                    <Pie
                        data={chartData}
                        cx="50%"
                        cy="50%"
                        innerRadius={60}
                        outerRadius={80}
                        paddingAngle={5}
                        dataKey="value"
                    >
                        {chartData.map((_, index) => (
                            <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                    </Pie>
                    <Tooltip
                        contentStyle={{
                            backgroundColor: '#111827',
                            border: '1px solid #374151',
                            borderRadius: '8px'
                        }}
                    />
                    <Legend verticalAlign="bottom" height={36} />
                </PieChart>
            </ResponsiveContainer>

            <div className="text-center mt-2">
                <p className="text-sm text-gray-400">
                    Ocupación promedio: {((totalVendidas / (totalVendidas + totalDisponibles || 1)) * 100).toFixed(1)}%
                </p>
            </div>
        </div>
    );
};
