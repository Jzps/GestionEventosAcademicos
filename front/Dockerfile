# Imagen del frontend Blazor del Sistema de Gestión de Eventos Académicos.
# Etapa 1: compilación con el SDK de .NET 10.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FrontGestionEventos.csproj ./
RUN dotnet restore FrontGestionEventos.csproj

COPY . ./
# El publish ejecuta su propio restore (los paquetes ya están en la capa anterior) para
# incluir correctamente los recursos estáticos del framework de Blazor (_framework).
RUN dotnet publish FrontGestionEventos.csproj -c Release -o /app/publish

# Etapa 2: imagen de ejecución.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

ENTRYPOINT ["dotnet", "FrontGestionEventos.dll"]
