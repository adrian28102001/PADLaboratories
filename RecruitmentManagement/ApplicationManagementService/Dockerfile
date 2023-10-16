FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ApplicationManagementService.csproj", "ApplicationManagementService/"]
RUN dotnet restore "ApplicationManagementService/ApplicationManagementService.csproj"
COPY . ApplicationManagementService
WORKDIR "/src/ApplicationManagementService"
RUN dotnet build "ApplicationManagementService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApplicationManagementService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApplicationManagementService.dll"]
