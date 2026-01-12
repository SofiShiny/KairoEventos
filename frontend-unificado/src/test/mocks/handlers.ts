import { http, HttpResponse } from 'msw';

const GATEWAY_URL = import.meta.env.VITE_GATEWAY_URL || 'http://localhost:8080';

/**
 * MSW Handlers for Gateway API endpoints
 * These handlers mock the Gateway responses for testing
 */
export const handlers = [
  // ============================================
  // EVENTOS ENDPOINTS
  // ============================================
  
  // GET /api/eventos - List all eventos
  http.get(`${GATEWAY_URL}/api/eventos`, () => {
    return HttpResponse.json({
      data: [
        {
          id: '1',
          nombre: 'Evento Test 1',
          descripcion: 'Descripción del evento test 1',
          fecha: '2024-12-31T20:00:00Z',
          ubicacion: 'Teatro Principal',
          imagenUrl: 'https://example.com/evento1.jpg',
          estado: 'Publicado',
          capacidadTotal: 100,
          asientosDisponibles: 50,
        },
        {
          id: '2',
          nombre: 'Evento Test 2',
          descripcion: 'Descripción del evento test 2',
          fecha: '2025-01-15T19:00:00Z',
          ubicacion: 'Auditorio Central',
          imagenUrl: 'https://example.com/evento2.jpg',
          estado: 'Publicado',
          capacidadTotal: 200,
          asientosDisponibles: 150,
        },
      ],
      success: true,
    });
  }),

  // GET /api/eventos/:id - Get evento by ID
  http.get(`${GATEWAY_URL}/api/eventos/:id`, ({ params }) => {
    const { id } = params;
    return HttpResponse.json({
      data: {
        id,
        nombre: `Evento Test ${id}`,
        descripcion: `Descripción del evento test ${id}`,
        fecha: '2024-12-31T20:00:00Z',
        ubicacion: 'Teatro Principal',
        imagenUrl: 'https://example.com/evento.jpg',
        estado: 'Publicado',
        capacidadTotal: 100,
        asientosDisponibles: 50,
      },
      success: true,
    });
  }),

  // POST /api/eventos - Create evento
  http.post(`${GATEWAY_URL}/api/eventos`, async ({ request }) => {
    const body = await request.json();
    return HttpResponse.json(
      {
        data: {
          id: 'new-evento-id',
          ...body,
          estado: 'Publicado',
          capacidadTotal: 0,
          asientosDisponibles: 0,
        },
        success: true,
        message: 'Evento creado exitosamente',
      },
      { status: 201 }
    );
  }),

  // PUT /api/eventos/:id - Update evento
  http.put(`${GATEWAY_URL}/api/eventos/:id`, async ({ params, request }) => {
    const { id } = params;
    const body = await request.json();
    return HttpResponse.json({
      data: {
        id,
        ...body,
      },
      success: true,
      message: 'Evento actualizado exitosamente',
    });
  }),

  // DELETE /api/eventos/:id - Cancel evento
  http.delete(`${GATEWAY_URL}/api/eventos/:id`, ({ params }) => {
    const { id } = params;
    return HttpResponse.json({
      data: { id, estado: 'Cancelado' },
      success: true,
      message: 'Evento cancelado exitosamente',
    });
  }),

  // ============================================
  // ENTRADAS ENDPOINTS
  // ============================================

  // GET /api/entradas/mis-entradas - Get user's entradas
  http.get(`${GATEWAY_URL}/api/entradas/mis-entradas`, ({ request }) => {
    const url = new URL(request.url);
    const estado = url.searchParams.get('estado');
    
    const allEntradas = [
      {
        id: '1',
        eventoId: '1',
        eventoNombre: 'Evento Test 1',
        asientoId: 'A1',
        asientoInfo: 'Fila A, Asiento 1',
        estado: 'Pagada',
        precio: 50.0,
        fechaCompra: '2024-12-01T10:00:00Z',
      },
      {
        id: '2',
        eventoId: '2',
        eventoNombre: 'Evento Test 2',
        asientoId: 'B5',
        asientoInfo: 'Fila B, Asiento 5',
        estado: 'Reservada',
        precio: 75.0,
        fechaCompra: '2024-12-15T14:30:00Z',
        tiempoRestante: 10,
      },
    ];

    const filteredEntradas = estado
      ? allEntradas.filter((e) => e.estado === estado)
      : allEntradas;

    return HttpResponse.json({
      data: filteredEntradas,
      success: true,
    });
  }),

  // GET /api/entradas/asientos-disponibles/:eventoId - Get available seats
  http.get(`${GATEWAY_URL}/api/entradas/asientos-disponibles/:eventoId`, ({ params }) => {
    const { eventoId } = params;
    return HttpResponse.json({
      data: [
        {
          id: 'A1',
          fila: 'A',
          numero: 1,
          estado: 'Disponible',
          precio: 50.0,
        },
        {
          id: 'A2',
          fila: 'A',
          numero: 2,
          estado: 'Reservado',
          precio: 50.0,
        },
        {
          id: 'A3',
          fila: 'A',
          numero: 3,
          estado: 'Ocupado',
          precio: 50.0,
        },
        {
          id: 'B1',
          fila: 'B',
          numero: 1,
          estado: 'Disponible',
          precio: 75.0,
        },
      ],
      success: true,
    });
  }),

  // POST /api/entradas - Create entrada
  http.post(`${GATEWAY_URL}/api/entradas`, async ({ request }) => {
    const body = await request.json();
    return HttpResponse.json(
      {
        data: {
          id: 'new-entrada-id',
          ...body,
          estado: 'Reservada',
          fechaCompra: new Date().toISOString(),
          tiempoRestante: 15,
        },
        success: true,
        message: 'Entrada creada exitosamente',
      },
      { status: 201 }
    );
  }),

  // DELETE /api/entradas/:id - Cancel entrada
  http.delete(`${GATEWAY_URL}/api/entradas/:id`, ({ params }) => {
    const { id } = params;
    return HttpResponse.json({
      data: { id, estado: 'Cancelada' },
      success: true,
      message: 'Entrada cancelada exitosamente',
    });
  }),

  // ============================================
  // USUARIOS ENDPOINTS (Admin only)
  // ============================================

  // GET /api/usuarios - List all usuarios
  http.get(`${GATEWAY_URL}/api/usuarios`, () => {
    return HttpResponse.json({
      data: [
        {
          id: '1',
          username: 'admin',
          nombre: 'Administrador',
          correo: 'admin@example.com',
          telefono: '+1234567890',
          rol: 'Admin',
          activo: true,
        },
        {
          id: '2',
          username: 'organizador1',
          nombre: 'Organizador Uno',
          correo: 'org1@example.com',
          telefono: '+1234567891',
          rol: 'Organizator',
          activo: true,
        },
      ],
      success: true,
    });
  }),

  // GET /api/usuarios/:id - Get usuario by ID
  http.get(`${GATEWAY_URL}/api/usuarios/:id`, ({ params }) => {
    const { id } = params;
    return HttpResponse.json({
      data: {
        id,
        username: `user${id}`,
        nombre: `Usuario ${id}`,
        correo: `user${id}@example.com`,
        telefono: '+1234567890',
        rol: 'Asistente',
        activo: true,
      },
      success: true,
    });
  }),

  // POST /api/usuarios - Create usuario
  http.post(`${GATEWAY_URL}/api/usuarios`, async ({ request }) => {
    const body = await request.json();
    return HttpResponse.json(
      {
        data: {
          id: 'new-usuario-id',
          ...body,
          activo: true,
        },
        success: true,
        message: 'Usuario creado exitosamente',
      },
      { status: 201 }
    );
  }),

  // PUT /api/usuarios/:id - Update usuario
  http.put(`${GATEWAY_URL}/api/usuarios/:id`, async ({ params, request }) => {
    const { id } = params;
    const body = await request.json();
    return HttpResponse.json({
      data: {
        id,
        ...body,
      },
      success: true,
      message: 'Usuario actualizado exitosamente',
    });
  }),

  // DELETE /api/usuarios/:id - Deactivate usuario
  http.delete(`${GATEWAY_URL}/api/usuarios/:id`, ({ params }) => {
    const { id } = params;
    return HttpResponse.json({
      data: { id, activo: false },
      success: true,
      message: 'Usuario desactivado exitosamente',
    });
  }),

  // ============================================
  // REPORTES ENDPOINTS (Admin/Organizator)
  // ============================================

  // GET /api/reportes/metricas-eventos - Get event metrics
  http.get(`${GATEWAY_URL}/api/reportes/metricas-eventos`, ({ request }) => {
    const url = new URL(request.url);
    const fechaInicio = url.searchParams.get('fechaInicio');
    const fechaFin = url.searchParams.get('fechaFin');
    
    return HttpResponse.json({
      data: [
        {
          eventoId: '1',
          eventoNombre: 'Evento Test 1',
          totalAsientos: 100,
          asientosVendidos: 50,
          ingresoTotal: 2500.0,
          tasaOcupacion: 50.0,
        },
        {
          eventoId: '2',
          eventoNombre: 'Evento Test 2',
          totalAsientos: 200,
          asientosVendidos: 50,
          ingresoTotal: 3750.0,
          tasaOcupacion: 25.0,
        },
      ],
      success: true,
    });
  }),

  // GET /api/reportes/historial-asistencia - Get attendance history
  http.get(`${GATEWAY_URL}/api/reportes/historial-asistencia`, () => {
    return HttpResponse.json({
      data: [
        {
          fecha: '2024-12-01',
          eventoNombre: 'Evento Test 1',
          asistentes: 50,
          noAsistentes: 10,
        },
        {
          fecha: '2024-12-15',
          eventoNombre: 'Evento Test 2',
          asistentes: 45,
          noAsistentes: 5,
        },
      ],
      success: true,
    });
  }),

  // GET /api/reportes/conciliacion-financiera - Get financial reconciliation
  http.get(`${GATEWAY_URL}/api/reportes/conciliacion-financiera`, () => {
    return HttpResponse.json({
      data: {
        totalIngresos: 6250.0,
        totalReembolsos: 250.0,
        ingresoNeto: 6000.0,
        entradasVendidas: 100,
        entradasCanceladas: 5,
      },
      success: true,
    });
  }),

  // POST /api/reportes/exportar - Export report
  http.post(`${GATEWAY_URL}/api/reportes/exportar`, async ({ request }) => {
    const body = await request.json();
    return HttpResponse.json({
      data: {
        url: 'https://example.com/reports/report-123.pdf',
        formato: body.formato || 'pdf',
      },
      success: true,
      message: 'Reporte exportado exitosamente',
    });
  }),

  // ============================================
  // DASHBOARD ENDPOINTS
  // ============================================

  // GET /api/dashboard/stats - Get dashboard statistics
  http.get(`${GATEWAY_URL}/api/dashboard/stats`, () => {
    return HttpResponse.json({
      data: {
        totalEventos: 10,
        misEntradas: 5,
        proximosEventos: 3,
        eventosDestacados: [
          {
            id: '1',
            nombre: 'Evento Destacado 1',
            fecha: '2024-12-31T20:00:00Z',
            ubicacion: 'Teatro Principal',
          },
        ],
      },
      success: true,
    });
  }),
];
