
## Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Ejecución de Pruebas Unitarias

Para ejecutar todas las pruebas unitarias del proyecto, abre una terminal en el directorio raíz de la solución y navega al directorio del proyecto de Pruebas

cd backend/src/Services/Eventos/Eventos.Pruebas

Y ejecuta el siguiente comando:


dotnet test


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

Este método levanta todo el entorno (API, base de datos, etc.) en contenedores, simulando un entorno de producción.

1.  Asegúrate de que Docker Desktop esté en ejecución.

2.  Abre una terminal en el directorio raíz de la solución.

3.  Ejecuta el siguiente comando para construir y levantar los contenedores:
  
    docker-compose up --build
   
4.  Una vez que los contenedores estén iniciados, la API estará disponible. Accede a la interfaz de Swagger en tu navegador:
 [http://localhost:5000/swagger](http://localhost:5000/swagger)