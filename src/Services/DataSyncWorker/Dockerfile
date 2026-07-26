# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for dependency restoration
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Services/DataSyncWorker/DataSyncWorker.csproj", "src/Services/DataSyncWorker/"]

RUN dotnet restore "src/Services/DataSyncWorker/DataSyncWorker.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/src/Services/DataSyncWorker"
RUN dotnet publish "DataSyncWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DataSyncWorker.dll"]