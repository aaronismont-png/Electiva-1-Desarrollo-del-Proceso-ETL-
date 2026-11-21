# ETL - Proceso de Extracción (.NET 8 Worker Service)

## Crear la solución
```
dotnet new sln -n EtlExtraction
dotnet sln add src/Etl.Domain src/Etl.Application src/Etl.Infrastructure src/Etl.Worker
dotnet build
```

## Configurar la cadena de conexión (sin subirla a GitHub)
```
cd src/Etl.Worker
dotnet user-secrets set "Extraction:Database:ConnectionString" "Server=.;Database=TuBD;Trusted_Connection=True;TrustServerCertificate=True"
```
Ajusta `Extraction:Database:Query` en appsettings.json a tu tabla de reseñas
(debe devolver: Id, ProductId, CustomerId, Rating, Text, Date, en ese orden).

## Ejecutar
```
dotnet run --project src/Etl.Worker
```
Los archivos extraídos se guardan en `Staging/` como JSON.
