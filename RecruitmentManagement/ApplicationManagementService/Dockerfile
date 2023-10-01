# Use the official Microsoft .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything and restore
COPY . ./
RUN dotnet restore ApplicationManagementService/ApplicationManagementService.csproj

# Publish
WORKDIR /app/ApplicationManagementService
RUN dotnet publish -c Release -o out

# Use the official Microsoft ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/ApplicationManagementService/out .

ENTRYPOINT ["dotnet", "ApplicationManagementService.dll"]
