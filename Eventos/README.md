# Microservicio de Eventos

Microservicio para la gestión de eventos utilizando arquitectura hexagonal, DDD y CQRS.

## 🚀 Características

- ✅ Arquitectura Hexagonal (Puertos y Adaptadores)
- ✅ Domain-Driven Design (DDD)
- ✅ CQRS con MediatR
- ✅ PostgreSQL para persistencia
- ✅ RabbitMQ para mensajería asíncrona
- ✅ Swagger/OpenAPI
- ✅ Docker support

## 📨 Integración con RabbitMQ

Este microservicio publica eventos de dominio a RabbitMQ usando MassTransit:

### Eventos Publicados

| Evento | Namespace | Propiedades |
|--------|-----------|-------------|
| **EventoPublicadoEventoDominio** | `Eventos.Dominio.EventosDeDominio` | EventoId, TituloEvento, FechaInicio |
| **AsistenteRegistradoEventoDominio** | `Eventos.Dominio.EventosDeDominio` | EventoId, UsuarioId, NombreUsuario |
| **EventoCanceladoEventoDominio** | `Eventos.Dominio.EventosDeDominio` | EventoId, TituloEvento |

### Configuración

Variable de entorno requerida:
```bash
RabbitMq:Host=localhost  # o la dirección de tu servidor RabbitMQ
```

Ver `INTEGRACION-RABBITMQ.md` para detalles completos de la integración.

## Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- RabbitMQ (incluido en docker-compose)

## Ejecución de Pruebas Unitarias

Para ejecutar todas las pruebas unitarias del proyecto, abre una terminal en el directorio raíz de la solución y navega al directorio del proyecto de Pruebas

cd backend/src/Services/Eventos/Eventos.Pruebas

Y ejecuta el siguiente comando:


dotnet test

o
bash

dotnet test Eventos.Pruebas.csproj   /p:CollectCoverage=true   /p:CoverletOutput=TestResults/coverage   /p:CoverletOutputFormat=cobertura   /p:Threshold=90   /p:ThresholdType=line   /p:ThresholdStat=total

Para generar reportes: 

reportgenerator   -reports:TestResults/**/coverage.cobertura.xml   -targetdir:coverage-report

explorer.exe "C:\Users\sofia\Source\Repos\Sistema-de-Eventos2\Eventos\backend\src\Services\Eventos\Eventos.Pruebas\coverage-report\index.html"

## Despliegue de la Aplicación

Puedes ejecutar la aplicación de dos maneras: directamente con el SDK de .NET o utilizando Docker Compose.

### Opción 1: Desarrollo Local


1.  Abre una terminal y navega al directorio del proyecto de la API:
  
    cd backend/src/Services/Eventos/Eventos.API
  

2.  Ejecuta la aplicación:

    dotnet run
   

3.  Una vez que la aplicación esté en ejecución, accede a la interfaz de Swagger en tu navegador:
    [http://localhost:5000/swagger](http://localhost:5000/swagger)

### Opción 2: Usando Docker Compose

Este método levanta todo el entorno (API, base de datos, RabbitMQ, etc.) en contenedores, simulando un entorno de producción.

1.  Asegúrate de que Docker Desktop esté en ejecución.

2.  Abre una terminal en el directorio raíz de la solución. (Sistema-de-Eventos2/Eventos)

3.  Ejecuta el siguiente comando para construir y levantar los contenedores:
  
    docker-compose up --build
   
4.  Una vez que los contenedores estén iniciados, la API estará disponible. Accede a la interfaz de Swagger en tu navegador:
 [http://localhost:5000/swagger](http://localhost:5000/swagger)

## 🌐 Endpoints API

### Eventos
- `GET /api/eventos` - Obtener todos los eventos
- `GET /api/eventos/{id}` - Obtener evento por ID
- `GET /api/eventos/organizador/{organizadorId}` - Obtener eventos por organizador
- `GET /api/eventos/publicados` - Obtener eventos publicados
- `POST /api/eventos` - Crear nuevo evento
- `PUT /api/eventos/{id}` - Actualizar evento
- `PATCH /api/eventos/{id}/publicar` - Publicar evento ✨ Publica a RabbitMQ
- `PATCH /api/eventos/{id}/cancelar` - Cancelar evento ✨ Publica a RabbitMQ
- `POST /api/eventos/{id}/asistentes` - Registrar asistente ✨ Publica a RabbitMQ
- `DELETE /api/eventos/{id}` - Eliminar evento

## 📚 Documentación Adicional

### 🚀 Inicio Rápido
- **QUICK-START-GUIDE.md** - Guía de inicio en 5 minutos
- **test-integracion.ps1** - Script automatizado de pruebas

### 📖 Documentación Técnica
- **INTEGRACION-RABBITMQ.md** - Detalles técnicos completos de la integración
- **ARQUITECTURA-INTEGRACION.md** - Diagramas de arquitectura y flujos de datos
- **VERIFICACION-INTEGRACION.md** - Guía paso a paso para verificar la integración

### 📋 Resúmenes y Planes
- **RESUMEN-INTEGRACION-RABBITMQ.md** - Resumen ejecutivo de la integración
- **RESUMEN-COMPLETO.md** - Resumen consolidado de todo el trabajo realizado
- **PLAN-SIGUIENTES-PASOS.md** - Plan detallado con tareas para continuar

### 🐳 Docker
- **docker-compose.rabbitmq.example.yml** - Ejemplo de configuración Docker Compose