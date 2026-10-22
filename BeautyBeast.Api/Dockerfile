# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + project files first for better layer caching.
# Only these files are needed for restore; source changes won't bust this layer.
COPY BeautyBeast.sln ./
COPY BeautyBeast.Api/BeautyBeast.Api.csproj BeautyBeast.Api/
COPY BeautyBeast.Web/BeautyBeast.Web.csproj BeautyBeast.Web/

RUN dotnet restore BeautyBeast.Api/BeautyBeast.Api.csproj

# Copy the rest of the API source and publish.
COPY BeautyBeast.Api/ BeautyBeast.Api/

RUN dotnet publish BeautyBeast.Api/BeautyBeast.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Pre-create the uploads directory so the volume mount lands in the right place.
RUN mkdir -p wwwroot/uploads

COPY --from=build /app/publish .

# aspnet:9.0 already sets ASPNETCORE_HTTP_PORTS=8080 — no need to override.
EXPOSE 8080

ENTRYPOINT ["dotnet", "BeautyBeast.Api.dll"]
