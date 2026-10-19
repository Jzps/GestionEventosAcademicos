# Imagen de la API ASP.NET Core del Sistema de Gestión de Eventos Académicos.
# Etapa 1: compilación con el SDK de .NET 10.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Se copian primero los archivos de proyecto para aprovechar la caché de capas de Docker.
COPY ApiGestionAcademico.csproj ./
RUN dotnet restore ApiGestionAcademico.csproj

COPY . ./
RUN dotnet publish ApiGestionAcademico.csproj -c Release -o /app/publish

# Etapa 2: imagen de ejecución.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl se usa para la comprobación de salud del contenedor.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiGestionAcademico.dll"]
