# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + project files first for better layer caching.
COPY BeautyBeast.sln ./
COPY BeautyBeast.Api/BeautyBeast.Api.csproj BeautyBeast.Api/
COPY BeautyBeast.Web/BeautyBeast.Web.csproj BeautyBeast.Web/

RUN dotnet restore BeautyBeast.Web/BeautyBeast.Web.csproj

# Copy the rest of the Web source and publish.
COPY BeautyBeast.Web/ BeautyBeast.Web/

RUN dotnet publish BeautyBeast.Web/BeautyBeast.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# aspnet:9.0 already sets ASPNETCORE_HTTP_PORTS=8080 — no need to override.
EXPOSE 8080

ENTRYPOINT ["dotnet", "BeautyBeast.Web.dll"]
