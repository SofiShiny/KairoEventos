
## Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Ejecuci�n de Pruebas Unitarias

Para ejecutar todas las pruebas unitarias del proyecto, abre una terminal en el directorio ra�z de la soluci�n y navega al directorio del proyecto de Pruebas

cd backend/src/Services/Eventos/Eventos.Pruebas

Y ejecuta el siguiente comando:


dotnet test

o
bash

dotnet test Eventos.Pruebas.csproj   /p:CollectCoverage=true   /p:CoverletOutput=TestResults/coverage   /p:CoverletOutputFormat=cobertura   /p:Threshold=90   /p:ThresholdType=line   /p:ThresholdStat=total

Para generar reportes: 

reportgenerator   -reports:TestResults/**/coverage.cobertura.xml   -targetdir:coverage-report

explorer.exe "C:\Users\sofia\Source\Repos\Sistema-de-Eventos2\Eventos\backend\src\Services\Eventos\Eventos.Pruebas\coverage-report\index.html"

## Despliegue de la Aplicaci�n

Puedes ejecutar la aplicaci�n de dos maneras: directamente con el SDK de .NET o utilizando Docker Compose.

### Opci�n 1: Desarrollo Local


1.  Abre una terminal y navega al directorio del proyecto de la API:
  
    cd backend/src/Services/Eventos/Eventos.API
  

2.  Ejecuta la aplicaci�n:

    dotnet run
   

3.  Una vez que la aplicaci�n est� en ejecuci�n, accede a la interfaz de Swagger en tu navegador:
    [http://localhost:5000/swagger](http://localhost:5000/swagger)

### Opci�n 2: Usando Docker Compose

Este m�todo levanta todo el entorno (API, base de datos, etc.) en contenedores, simulando un entorno de producci�n.

1.  Aseg�rate de que Docker Desktop est� en ejecuci�n.

2.  Abre una terminal en el directorio ra�z de la soluci�n. (Sistema-de-Eventos2/Eventos)

3.  Ejecuta el siguiente comando para construir y levantar los contenedores:
  
    docker-compose up --build
   
4.  Una vez que los contenedores est�n iniciados, la API estar� disponible. Accede a la interfaz de Swagger en tu navegador:
 [http://localhost:5000/swagger](http://localhost:5000/swagger)