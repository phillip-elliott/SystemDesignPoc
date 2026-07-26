# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Web/BlazorUi/BlazorUi.csproj", "src/Web/BlazorUi/"]

RUN dotnet restore "src/Web/BlazorUi/BlazorUi.csproj"

COPY . .
WORKDIR "/src/src/Web/BlazorUi"
RUN dotnet publish "BlazorUi.csproj" -c Release -o /app/publish

# Runtime stage using NGINX Alpine
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

RUN rm -rf /usr/share/nginx/html/* /etc/nginx/conf.d/default.conf

COPY --from=build /app/publish/wwwroot .
COPY src/Web/BlazorUi/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]