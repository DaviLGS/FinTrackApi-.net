# Build a partir da raiz do repositório:
#   docker build -f src/FinTrack.Api/Dockerfile .

# ---------- Etapa 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro para aproveitar o cache do restore
COPY src/FinTrack.Domain/FinTrack.Domain.csproj src/FinTrack.Domain/
COPY src/FinTrack.Application/FinTrack.Application.csproj src/FinTrack.Application/
COPY src/FinTrack.Infrastructure/FinTrack.Infrastructure.csproj src/FinTrack.Infrastructure/
COPY src/FinTrack.Api/FinTrack.Api.csproj src/FinTrack.Api/
RUN dotnet restore src/FinTrack.Api/FinTrack.Api.csproj

COPY src/ src/
RUN dotnet publish src/FinTrack.Api/FinTrack.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Usuário não-root já existente na imagem oficial
USER $APP_UID

ENTRYPOINT ["dotnet", "FinTrack.Api.dll"]
