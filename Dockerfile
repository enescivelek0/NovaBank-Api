# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for better Docker layer caching
COPY Banking.sln ./
COPY src/Banking.Domain/Banking.Domain.csproj src/Banking.Domain/
COPY src/Banking.Application/Banking.Application.csproj src/Banking.Application/
COPY src/Banking.Infrastructure/Banking.Infrastructure.csproj src/Banking.Infrastructure/
COPY src/Banking.API/Banking.API.csproj src/Banking.API/
COPY tests/Banking.UnitTests/Banking.UnitTests.csproj tests/Banking.UnitTests/

RUN dotnet restore Banking.sln

# Copy all source files
COPY . .

# Run tests during docker build to ensure code quality
RUN dotnet test tests/Banking.UnitTests/Banking.UnitTests.csproj --configuration Release --no-restore

# Publish API
RUN dotnet publish src/Banking.API/Banking.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Banking.API.dll"]
