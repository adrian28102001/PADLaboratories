FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 81
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["JobManagementService.csproj", "JobManagementService/"]
RUN dotnet restore "JobManagementService/JobManagementService.csproj"
COPY . JobManagementService
WORKDIR "/src/JobManagementService"
RUN dotnet build "JobManagementService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JobManagementService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobManagementService.dll"]
