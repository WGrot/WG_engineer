﻿FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RestaurantApp.Api/RestaurantApp.Api.csproj", "RestaurantApp.Api/"]
COPY ["RestaurantApp.Shared/RestaurantApp.Shared.csproj", "RestaurantApp.Shared/"]
RUN dotnet restore "RestaurantApp.Api/RestaurantApp.Api.csproj"
COPY . .
WORKDIR "/src/RestaurantApp.Api"
RUN dotnet build "RestaurantApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RestaurantApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestaurantApp.Api.dll"]
