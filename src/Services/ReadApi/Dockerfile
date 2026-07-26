# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for dependency restoration
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Services/ReadApi/ReadApi.csproj", "src/Services/ReadApi/"]

RUN dotnet restore "src/Services/ReadApi/ReadApi.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/src/Services/ReadApi"
RUN dotnet publish "ReadApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReadApi.dll"]