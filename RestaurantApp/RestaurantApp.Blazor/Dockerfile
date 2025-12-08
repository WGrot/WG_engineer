# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["RestaurantApp.Blazor/RestaurantApp.Blazor.csproj", "RestaurantApp.Blazor/"]
COPY ["RestaurantApp.Shared/RestaurantApp.Shared.csproj", "RestaurantApp.Shared/"]
RUN dotnet restore "RestaurantApp.Blazor/RestaurantApp.Blazor.csproj"

# Copy everything and publish
COPY . .
WORKDIR "/src/RestaurantApp.Blazor"
RUN dotnet publish -c Release -o /app/publish

# Runtime stage - nginx
FROM nginx:alpine AS final

# Remove default nginx content
RUN rm -rf /usr/share/nginx/html/*

# Copy published Blazor app
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Copy nginx configuration
COPY RestaurantApp.Blazor/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]