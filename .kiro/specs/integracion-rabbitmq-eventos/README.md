# Spec: Integración RabbitMQ en Microservicio de Eventos

## 📋 Overview

Este spec documenta la integración completa de RabbitMQ en el microservicio de Eventos, incluyendo la implementación base (ya completada), verificación, pruebas y mejoras opcionales para producción.

## 🎯 Objetivo

Completar y verificar la integración de RabbitMQ en el microservicio de Eventos para permitir comunicación asíncrona con otros microservicios mediante eventos de dominio.

## 📊 Estado Actual

**Progreso:** 10% Completado

| Fase | Estado | Descripción |
|------|--------|-------------|
| Implementación Base | ✅ Completada | Configuración de MassTransit y modificación de handlers |
| Verificación Local | ⏳ Pendiente | Pruebas locales y validación de mensajes |
| Integración Reportes | ⏳ Pendiente | Actualización de contratos y consumidores |
| Pruebas E2E | ⏳ Pendiente | Pruebas end-to-end completas |
| Resiliencia | ⏳ Pendiente | Pruebas de reconexión y carga |
| Docker Compose | ⏳ Pendiente | Configuración de despliegue completo |
| Mejoras Opcionales | ⚠️ Futuro | Outbox, Retry, DLQ, Observabilidad |

## 📚 Documentos del Spec

### 1. requirements.md
Define los requisitos funcionales y no funcionales de la integración:
- 10 requisitos principales
- Matriz de prioridades
- Criterios de aceptación detallados

### 2. design.md
Describe el diseño técnico de la solución:
- Arquitectura de componentes
- Flujos de datos
- Propiedades de correctness
- Estrategia de testing
- Configuración y deployment

### 3. tasks.md
Plan de implementación con tareas específicas:
- 12 tareas principales
- Subtareas detalladas
- Referencias a requirements
- Tareas opcionales marcadas con `*`

## 🚀 Quick Start

### Para Desarrolladores

1. **Revisar el estado actual:**
   ```bash
   # Ver documentación de lo ya implementado
   cat Eventos/RESUMEN-COMPLETO.md
   ```

2. **Siguiente paso recomendado:**
   ```bash
   # Ejecutar verificación local
   cd Eventos
   .\test-integracion.ps1
   ```

3. **Seguir el plan:**
   - Abrir `tasks.md`
   - Comenzar con Task 2: Verificación Local
   - Marcar subtareas completadas

### Para Arquitectos

1. **Revisar diseño:**
   - Leer `design.md` completo
   - Revisar arquitectura de componentes
   - Validar propiedades de correctness

2. **Evaluar mejoras opcionales:**
   - Outbox Pattern (Task 8)
   - Retry Policies (Task 9)
   - Dead Letter Queues (Task 10)
   - Observabilidad (Task 11)

## 🔑 Conceptos Clave

### Eventos de Dominio Publicados

| Evento | Namespace | Cuándo se Publica |
|--------|-----------|-------------------|
| EventoPublicadoEventoDominio | Eventos.Dominio.EventosDeDominio | Al publicar un evento |
| AsistenteRegistradoEventoDominio | Eventos.Dominio.EventosDeDominio | Al registrar un asistente |
| EventoCanceladoEventoDominio | Eventos.Dominio.EventosDeDominio | Al cancelar un evento |

### Patrón de Publicación

```
1. Lógica de Dominio (validaciones)
   ↓
2. Persistencia en PostgreSQL
   ↓
3. Publicación a RabbitMQ
```

**Importante:** La persistencia SIEMPRE ocurre antes de la publicación.

## 📖 Documentación Relacionada

### En el Repositorio de Eventos

- `INTEGRACION-RABBITMQ.md` - Detalles técnicos completos
- `RESUMEN-INTEGRACION-RABBITMQ.md` - Resumen ejecutivo
- `QUICK-START-GUIDE.md` - Guía de inicio rápido
- `VERIFICACION-INTEGRACION.md` - Guía de verificación
- `ARQUITECTURA-INTEGRACION.md` - Diagramas de arquitectura
- `PLAN-SIGUIENTES-PASOS.md` - Plan detallado de continuación
- `test-integracion.ps1` - Script de pruebas automatizado

### Specs Relacionados

- `.kiro/specs/microservicio-reportes/` - Spec del microservicio de Reportes
- `.kiro/specs/integracion-rabbitmq-asientos/` - Spec para integración con Asientos

## 🎯 Próximos Pasos

### Inmediatos (Alta Prioridad)

1. **Task 2: Verificación Local**
   - Ejecutar `test-integracion.ps1`
   - Verificar mensajes en RabbitMQ UI
   - Documentar resultados

2. **Task 3: Actualización de Reportes**
   - Sincronizar contratos
   - Crear EventoCanceladoConsumer
   - Compilar y verificar

3. **Task 4: Pruebas E2E**
   - Levantar entorno completo
   - Ejecutar pruebas end-to-end
   - Validar flujo completo

### Mediano Plazo (Media Prioridad)

4. **Task 5: Pruebas de Resiliencia**
   - Prueba de reconexión
   - Prueba de carga

5. **Task 6: Docker Compose**
   - Configuración completa
   - Documentación de uso

### Largo Plazo (Baja Prioridad - Opcional)

6. **Tasks 8-11: Mejoras de Producción**
   - Outbox Pattern
   - Retry Policies
   - Dead Letter Queues
   - Observabilidad

## 🧪 Testing

### Tipos de Pruebas

1. **Unit Tests:** Verifican componentes individuales
2. **Property Tests:** Verifican propiedades universales (mínimo 100 iteraciones)
3. **Integration Tests:** Verifican comunicación entre servicios
4. **E2E Tests:** Verifican flujos completos

### Ejecutar Pruebas

```powershell
# Pruebas automatizadas de integración
.\test-integracion.ps1

# Pruebas unitarias
cd Eventos/backend/src/Services/Eventos/Eventos.Pruebas
dotnet test

# Pruebas con cobertura
dotnet test /p:CollectCoverage=true
```

## 📊 Métricas de Éxito

### Implementación Base (✅ Completada)

- [x] 3 eventos de dominio publicándose
- [x] 2 handlers modificados
- [x] 1 nuevo handler creado
- [x] Compilación exitosa
- [x] Documentación completa

### Verificación y Pruebas (⏳ Pendiente)

- [ ] Script de pruebas ejecutado exitosamente
- [ ] Mensajes verificados en RabbitMQ
- [ ] Contratos sincronizados en Reportes
- [ ] EventoCanceladoConsumer implementado
- [ ] Pruebas E2E pasando
- [ ] Pruebas de resiliencia completadas

### Mejoras Opcionales (⚠️ Futuro)

- [ ] Outbox Pattern implementado
- [ ] Retry Policies configuradas
- [ ] Dead Letter Queues configuradas
- [ ] Observabilidad implementada

## 🤝 Contribuir

### Para Agregar Tareas

1. Editar `tasks.md`
2. Agregar subtarea con checkbox
3. Referenciar requirement correspondiente
4. Marcar como opcional con `*` si aplica

### Para Actualizar Diseño

1. Editar `design.md`
2. Actualizar diagramas si es necesario
3. Agregar propiedades de correctness si aplica
4. Documentar decisiones de diseño

### Para Agregar Requirements

1. Editar `requirements.md`
2. Seguir formato EARS
3. Agregar criterios de aceptación
4. Actualizar matriz de prioridades

## 📞 Soporte

Para preguntas o problemas:

1. Revisar documentación en `Eventos/`
2. Consultar `QUICK-START-GUIDE.md`
3. Revisar `VERIFICACION-INTEGRACION.md`
4. Ejecutar `test-integracion.ps1` para diagnóstico

## 🔗 Enlaces Útiles

- [MassTransit Documentation](https://masstransit-project.com/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Microservices Patterns](https://microservices.io/patterns/)

---

**Última Actualización:** 29 de Diciembre de 2024  
**Versión:** 1.0  
**Estado:** En Progreso (10% Completado)
