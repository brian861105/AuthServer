# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only the main project files (not solution file)
COPY src/Domain/AuthServer.Domain.csproj src/Domain/
COPY src/Application/AuthServer.Application.csproj src/Application/
COPY src/Infrastructure/AuthServer.Infrastructure.csproj src/Infrastructure/
COPY src/API/AuthServer.API.csproj src/API/

# Restore dependencies for the main project only
RUN dotnet restore src/API/AuthServer.API.csproj

# Copy source code
COPY src/ src/

# Build the application
RUN dotnet publish src/API/AuthServer.API.csproj -c Release -o /app/publish

# Use the official .NET runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "AuthServer.API.dll"]