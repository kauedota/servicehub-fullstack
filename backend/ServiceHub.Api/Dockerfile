# =====================================================================
# Dockerfile da API — "multi-stage build"
#  Estágio 1 (build): imagem GRANDE com o SDK (compilador) -> gera os binários
#  Estágio 2 (final): imagem PEQUENA só com o runtime ASP.NET -> roda os binários
# Mesmo conceito de usar maven:3-eclipse-temurin para buildar e eclipse-temurin:21-jre para rodar.
#
# O contexto do build é a RAIZ do repositório (por causa da pasta /database):
#   docker build -f backend/ServiceHub.Api/Dockerfile -t servicehub-api .
# =====================================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) Copia só o .csproj e restaura pacotes. Essa camada fica em CACHE enquanto as dependências não mudarem.
COPY backend/ServiceHub.Api/ServiceHub.Api.csproj backend/ServiceHub.Api/
RUN dotnet restore backend/ServiceHub.Api/ServiceHub.Api.csproj

# 2) Copia o código e os scripts SQL e publica
COPY backend/ServiceHub.Api/ backend/ServiceHub.Api/
COPY database/ database/
RUN dotnet publish backend/ServiceHub.Api/ServiceHub.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# A imagem oficial do .NET 8 escuta na porta 8080 e traz um usuário sem privilégios ("app").
EXPOSE 8080
COPY --from=build /app/publish .
# Segurança: não rodar como root dentro do container.
USER $APP_UID
ENTRYPOINT ["dotnet", "ServiceHub.Api.dll"]
