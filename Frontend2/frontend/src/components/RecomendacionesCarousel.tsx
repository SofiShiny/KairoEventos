import React from 'react';
import { EventoRecomendado } from '../services/recomendaciones.service';
import { useNavigate } from 'react-router-dom';
import { Calendar, MapPin, TrendingUp } from 'lucide-react';
import './RecomendacionesCarousel.css';

interface RecomendacionesCarouselProps {
    titulo: string;
    eventos: EventoRecomendado[];
    subtitulo?: string;
}

export const RecomendacionesCarousel: React.FC<RecomendacionesCarouselProps> = ({
    titulo,
    eventos,
    subtitulo
}) => {
    const navigate = useNavigate();

    const formatearFecha = (fecha: string) => {
        const date = new Date(fecha);
        return date.toLocaleDateString('es-ES', {
            day: 'numeric',
            month: 'short',
            year: 'numeric'
        });
    };

    const formatearPrecio = (precio: number) => {
        if (precio === 0) return 'Gratis';
        return new Intl.NumberFormat('es-CL', {
            style: 'currency',
            currency: 'CLP',
            minimumFractionDigits: 0
        }).format(precio);
    };

    if (!eventos || eventos.length === 0) {
        return null;
    }

    return (
        <section className="recomendaciones-section">
            <div className="recomendaciones-header">
                <div>
                    <h2 className="recomendaciones-titulo">{titulo}</h2>
                    {subtitulo && <p className="recomendaciones-subtitulo">{subtitulo}</p>}
                </div>
            </div>

            <div className="recomendaciones-carousel">
                <div className="carousel-track">
                    {eventos.map((evento) => (
                        <div
                            key={evento.id}
                            className="carousel-card"
                            onClick={() => navigate(`/eventos/${evento.id}`)}
                        >
                            {/* Imagen del evento */}
                            <div className="card-image-container">
                                {evento.urlImagen ? (
                                    <img
                                        src={evento.urlImagen}
                                        alt={evento.titulo}
                                        className="card-image"
                                    />
                                ) : (
                                    <div className="card-image-placeholder">
                                        <Calendar size={40} />
                                    </div>
                                )}

                                {/* Badge de categoría */}
                                {evento.categoria && (
                                    <div className="card-badge">{evento.categoria}</div>
                                )}

                                {/* Badge de popularidad */}
                                {evento.entradasVendidas > 50 && (
                                    <div className="card-badge-popular">
                                        <TrendingUp size={14} />
                                        Popular
                                    </div>
                                )}
                            </div>

                            {/* Contenido de la tarjeta */}
                            <div className="card-content">
                                <h3 className="card-titulo">{evento.titulo}</h3>

                                <div className="card-info">
                                    <div className="card-info-item">
                                        <Calendar size={16} />
                                        <span>{formatearFecha(evento.fechaInicio)}</span>
                                    </div>

                                    {evento.ciudad && (
                                        <div className="card-info-item">
                                            <MapPin size={16} />
                                            <span>{evento.ciudad}</span>
                                        </div>
                                    )}
                                </div>

                                {evento.descripcion && (
                                    <p className="card-descripcion">
                                        {evento.descripcion.length > 80
                                            ? `${evento.descripcion.substring(0, 80)}...`
                                            : evento.descripcion}
                                    </p>
                                )}

                                <div className="card-footer">
                                    <div className="card-precio">
                                        <span className="precio-label">Desde</span>
                                        <span className="precio-valor">{formatearPrecio(evento.precioDesde)}</span>
                                    </div>

                                    {evento.entradasVendidas > 0 && (
                                        <div className="card-ventas">
                                            {evento.entradasVendidas} vendidas
                                        </div>
                                    )}
                                </div>
                            </div>

                            {/* Efecto hover */}
                            <div className="card-hover-overlay">
                                <button className="card-hover-button">Ver Detalles</button>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
};
