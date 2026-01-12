# Microservicio de Encuestas - Kairo Eventos

Este microservicio se encarga de gestionar el feedback de los usuarios post-evento y validar la asistencia mediante encuestas rápidas. Es parte del ecosistema de microservicios de **Kairo Eventos**.

## 🚀 Tecnologías Utilizadas

- **.NET 8.0**
- **Entity Framework Core** (PostgreSQL)
- **MediatR** (Patrón CQRS)
- **MassTransit** con **RabbitMQ** (Comunicación asíncrona)
- **xUnit** & **FluentAssertions** (Pruebas unitarias)

## 📁 Estructura del Proyecto

El microservicio sigue los principios de **Clean Architecture**:

- **Encuestas.API**: Punto de entrada del servicio. Contiene los controladores y la configuración de Docker.
- **Encuestas.Aplicacion**: Contiene la lógica de negocio, comandos y consultas (CQRS).
- **Encuestas.Dominio**: Entidades del corazón del sistema, interfaces de repositorios y lógica central.
- **Encuestas.Infraestructura**: Implementación de la persistencia (DB Context), repositorios y servicios externos.
- **Encuestas.Tests**: Suite de pruebas unitarias e integración.

## 🛠️ Cómo Ejecutar

### Requisitos Previos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Docker Desktop (si se desea ejecutar en contenedores)

### Ejecución Local

1. Configurar la cadena de conexión en `Encuestas.API/appsettings.json`.
2. Ejecutar las migraciones:
   ```bash
   dotnet ef database update --project Encuestas.Infraestructura --startup-project Encuestas.API
   ```
3. Iniciar el servicio:
   ```bash
   dotnet run --project Encuestas.API
   ```

### Ejecución con Docker

El servicio está integrado en el `docker-compose.yml` principal de la carpeta `Infraestructura`. Para levantarlo individualmente:

```bash
docker compose up -d encuestas-api --build
```

## 🧪 Pruebas

Para ejecutar las pruebas y generar resultados:

```bash
dotnet test --results-directory ./testresults
```

---
© 2026 Kairo Eventos - Sistema de Gestión de Eventos.
