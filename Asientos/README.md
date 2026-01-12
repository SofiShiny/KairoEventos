# 🎫 Microservicio de Asientos

Microservicio para gestión de mapas de asientos, categorías, reservas y liberaciones en eventos.

## 🏗️ Arquitectura

- **Patrón:** Hexagonal (Ports & Adapters)
- **CQRS:** Separación estricta Commands/Queries con MediatR
- **Event-Driven:** Publicación de eventos de dominio a RabbitMQ con MassTransit
- **Base de Datos:** PostgreSQL con Entity Framework Core
- **Identificadores:** GUIDs

## 📦 Estructura del Proyecto

```
Asientos/
├── Asientos.API/              # Capa de presentación (Controllers, Program.cs)
├── Asientos.Aplicacion/       # Capa de aplicación (Commands, Queries, Handlers)
├── Asientos.Dominio/          # Capa de dominio (Agregados, Entidades, Eventos)
├── Asientos.Infraestructura/  # Capa de infraestructura (Repositorios, DbContext)
└── Asientos.Pruebas/          # Tests unitarios y de integración
```

## 🚀 Características

### **Commands (Escritura):**
- ✅ Crear mapa de asientos para un evento
- ✅ Agregar categoría a un mapa
- ✅ Agregar asiento a un mapa
- ✅ Reservar asiento
- ✅ Liberar asiento

### **Queries (Lectura):**
- ✅ Obtener mapa de asientos con categorías y asientos

### **Eventos de Dominio Publicados:**
1. `MapaAsientosCreadoEventoDominio`
2. `CategoriaAgregadaEventoDominio`
3. `AsientoAgregadoEventoDominio`
4. `AsientoReservadoEventoDominio`
5. `AsientoLiberadoEventoDominio`

## 🔧 Configuración

### **Variables de Entorno:**

```bash
# PostgreSQL
POSTGRES_HOST=localhost
POSTGRES_DB=asientosdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5432

# RabbitMQ (opcional, usa appsettings.json por defecto)
RabbitMq__Host=localhost
```

### **appsettings.json:**

```json
{
  "RabbitMq": {
    "Host": "localhost"
  }
}
```

## 🐳 Docker Compose

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: asientosdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
  
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
  
  asientos-api:
    build: .
    environment:
      POSTGRES_HOST: postgres
      POSTGRES_DB: asientosdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      RabbitMq__Host: rabbitmq
    ports:
      - "5000:8080"
    depends_on:
      - postgres
      - rabbitmq
```

## 🏃 Ejecución

### **Desarrollo Local:**

```bash
# 1. Levantar infraestructura
docker-compose up -d postgres rabbitmq

# 2. Restaurar paquetes
cd backend/src/Services/Asientos/Asientos.API
dotnet restore

# 3. Ejecutar API
dotnet run
```

### **Con Docker:**

```bash
docker-compose up
```

## 📡 API Endpoints

### **Mapas de Asientos:**

```http
POST   /api/asientos/mapas          # Crear mapa
GET    /api/asientos/mapas/{id}     # Obtener mapa
```

### **Categorías:**

```http
POST   /api/asientos/categorias     # Agregar categoría
```

### **Asientos:**

```http
POST   /api/asientos                # Agregar asiento
POST   /api/asientos/reservar       # Reservar asiento
POST   /api/asientos/liberar        # Liberar asiento
```

### **Health Check:**

```http
GET    /health                      # Estado del servicio
```

## 📚 Swagger

Acceder a la documentación interactiva:
```
http://localhost:5000/swagger
```

## 🔍 RabbitMQ Management

Acceder a la consola de administración:
```
http://localhost:15672
Usuario: guest
Password: guest
```

## 🧪 Tests

```bash
cd backend/src/Services/Asientos/Asientos.Pruebas
dotnet test
```

## 📖 Documentación Adicional

- [Refactorización CQRS + RabbitMQ](./REFACTORIZACION-CQRS-RABBITMQ.md) - Documento técnico completo
- [Resumen Ejecutivo](./RESUMEN-EJECUTIVO-REFACTORIZACION.md) - Resumen de cambios

## 🏛️ Principios de Diseño

### **CQRS Estricto:**
- Commands retornan solo `Guid` o `Unit`
- Queries retornan DTOs inmutables
- Separación completa entre escritura y lectura

### **Controladores "Thin":**
- Sin lógica de negocio
- Solo orquestación con MediatR
- Sin construcción manual de ViewModels

### **Event-Driven:**
- Patrón: Save → Publish
- Eventos inmutables
- Publicación asíncrona a RabbitMQ

### **Arquitectura Hexagonal:**
- Dominio independiente de infraestructura
- Inversión de dependencias
- Puertos y adaptadores

## 🔐 Seguridad

- ✅ Validación de entrada con Data Annotations
- ✅ Manejo de excepciones centralizado
- ✅ CORS configurado para desarrollo
- ⚠️ Autenticación/Autorización pendiente (próxima fase)

## 📊 Monitoreo

### **Health Check:**
```bash
curl http://localhost:5000/health
```

**Respuesta:**
```json
{
  "status": "healthy",
  "db": "postgres",
  "rabbitmq": "localhost"
}
```

## 🚧 Próximos Pasos

1. Implementar autenticación con JWT
2. Agregar logging estructurado (Serilog)
3. Implementar retry policies en MassTransit
4. Agregar métricas con Prometheus
5. Implementar circuit breaker
6. Agregar tests de integración con RabbitMQ

## 📝 Notas Técnicas

- **MassTransit v8.1.3** utiliza convenciones automáticas para exchanges/queues
- Los eventos se publican al exchange `Asientos.Dominio.EventosDominio:NombreEvento`
- Entity Framework Core aplica migraciones automáticamente al iniciar
- Soporte para InMemory database para desarrollo sin PostgreSQL

## 🤝 Contribución

1. Fork el proyecto
2. Crear feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push al branch (`git push origin feature/AmazingFeature`)
5. Abrir Pull Request

## 📄 Licencia

Este proyecto es parte del Sistema de Eventos.

---

**Última actualización:** 29 de Diciembre de 2024  
**Versión:** 2.0.0 (Refactorización CQRS + RabbitMQ)
